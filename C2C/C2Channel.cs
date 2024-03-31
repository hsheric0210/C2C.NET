using C2C.Handshake;
using C2C.Handshake.Encoder;
using C2C.Handshake.Generator;
using C2C.Medium;
using C2C.Message;
using C2C.Message.Encoder;
using C2C.Processor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace C2C
{
    public class C2Channel : IChannel
    {
        public static readonly Guid HandshakeMessageID = Guid.Parse("7C785171-C75C-4F55-AA54-0C78C9EF2DA2");

        /// <inheritdoc/>
        public bool CanReceive => medium.CanReceive;

        /// <inheritdoc/>
        public bool CanTransmit => medium.CanTransmit;

        public bool Connected => medium.Connected;

        /// <inheritdoc/>
        public Guid ChannelId { get; }

        public event EventHandler<DataEventArgs> OnHandshake;
        public event EventHandler<DataEventArgs> OnReceive;

        private readonly IMedium medium;
        private readonly IMessageEncoder encoder;
        private readonly IHandshakeEncoder handshakeEncoder;

        private readonly IHandshake handshake;
        private readonly byte[] handshakeBytes;

        private IProcessor[] processors;
        private IProcessor[] processorsReversed;

        private bool enableProcessors = false;
        private bool disposedValue;

        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>> receiveHandlerQueue
            = new ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>>();

        /// <summary>
        /// Creates a C2 channel.
        /// </summary>
        /// <param name="channelId">Unique identifier for this kind of communication channels. (Each program or project should have distinctive channel ID)</param>
        /// <param name="medium">Medium to communicate with.</param>
        /// <param name="encoder">Message packet encoder to use.</param>
        /// <param name="handshakeGenerator">Handshake packet generator to use in handshaking phase.</param>
        /// <param name="handshakeEncoder">Handshake packet encoder to use in handshaking phase.</param>
        /// <param name="processors">A list of processors to use. Only processors that are supported by both server and client will be actually enabled.</param>
        public C2Channel(Guid channelId, IMedium medium, IMessageEncoder encoder, IHandshakeGenerator handshakeGenerator, IHandshakeEncoder handshakeEncoder, IProcessor[] processors)
        {
            this.medium = medium;
            this.encoder = encoder;
            this.handshakeEncoder = handshakeEncoder;
            this.processors = processors;

            handshake = handshakeGenerator.Generate(channelId, processors);
            handshakeBytes = handshakeEncoder.Encode(handshake);

            medium.OnReceive += Medium_OnReceive;
            ChannelId = channelId;
        }

        private void SetProcessors(IProcessor[] processors)
        {
            this.processors = processors;

            processorsReversed = processors.Clone() as IProcessor[];
            Array.Reverse(processorsReversed);
        }

        private void Medium_OnReceive(object sender, RawPacketEventArgs e)
        {
            var packet = encoder.Decode(e.RawPacket);

            Logging.Log("Received channel packet. (Message ID: {0}, Decoded Size: {1})", packet.MessageID, packet.Data.Length);

            var isHandshake = packet.MessageID == HandshakeMessageID;

            // Process asynchronously
            Task.Run(() =>
            {
                try
                {
                    using (var sha256 = SHA256.Create())
                    {
                        var hash = sha256.ComputeHash(packet.Data);
                        if (!hash.SequenceEqual(packet.DataHash)) // todo: fix potential timing attack vulnerability
                        {
                            Logging.Log("Dropped a packet with mismatched data hash. (MessageID={0})", packet.MessageID);
                            return; // Drop corrupted or tampered data
                        }
                    }

                    var data = packet.Data;

                    if (!isHandshake && enableProcessors && processorsReversed != null)
                    {
                        foreach (var processor in processorsReversed) // We must apply processors in reversed order to properly process.
                            data = processor.ProcessIncomingData(data);
                    }

                    if (isHandshake)
                        HandleHandshake(packet);

                    if (receiveHandlerQueue.TryRemove(packet.MessageID, out var taskCompletion))
                    {
                        taskCompletion.SetResult(data);
                    }

                    OnReceive?.Invoke(this, new DataEventArgs(packet.MessageID, data));
                }
                catch (Exception ex)
                {
                    Logging.Log("Error handling received packet. {0}", ex.ToString());
                }
            });
        }

        /// <inheritdoc/>
        public void Transmit(Guid messageId, byte[] data)
        {
            Task.Run(() =>
            {
                try
                {
                    if (enableProcessors)
                    {
                        foreach (var processor in processors)
                            data = processor.ProcessOutgoingData(data);
                    }

                    var packetBytes = encoder.Encode(messageId, data);
                    medium.Transmit(packetBytes);
                }
                catch (Exception ex)
                {
                    Logging.Log("Error transmitting packet. {0}", ex.ToString());
                }
            });
        }

        /// <inheritdoc/>
        public async Task<byte[]> Transceive(Guid messageId, byte[] data, TimeSpan timeout)
        {
            Transmit(messageId, data);
            return await WaitForResponse(messageId, timeout);
        }

        /// <inheritdoc/>
        public async Task<byte[]> WaitForResponse(Guid messageId, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            // Enqueue the response wait
            receiveHandlerQueue[messageId] = tcs;

            // Wait until the response received
            return await Task.Run(() =>
            {
                tcs.Task.Wait(timeout);
                if (!tcs.Task.IsCompleted)
                    throw new TimeoutException();
                return tcs.Task.Result;
            });
        }

        /// <inheritdoc/>
        public async Task Open()
        {
            await medium.Open();
            Transmit(HandshakeMessageID, handshakeBytes);
        }

        private void HandleHandshake(IMessagePacket packet)
        {
            var handshakeData = packet.Data;

            Logging.Log("A handshake packet received.");

            OnHandshake?.Invoke(this, new DataEventArgs(HandshakeMessageID, handshakeData));

            var handshakeResponse = handshakeEncoder.Decode(handshakeData);
            if (handshakeResponse.ChannelId != ChannelId)
                throw new IOException("Unsupported channel id");

            // Only enable processors that are supported in both sides.
            var succeedNegotiations = handshakeResponse.ProcessorNegotiations.Intersect(handshake.ProcessorNegotiations).ToArray();

            // Set up processors
            var filteredProcessorList = new List<IProcessor>(processors.Length);
            foreach (var processor in processors)
            {
                foreach (var negotiation in succeedNegotiations)
                {
                    if (negotiation.ProcessorId != processor.ProcessorID)
                        continue;

                    processor.FinishNegotiate(negotiation.ProcessorParameter);
                    filteredProcessorList.Add(processor);
                    break;
                }
            }

            // Update processor list
            SetProcessors(filteredProcessorList.ToArray());

            Logging.Log("Negotiation finished. Enabling processors.");

            // Negotiation finished; enable processors
            enableProcessors = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    medium.Dispose();
                }

                processors = null;
                processorsReversed = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
