using C2C.Handshake.Encoder;
using C2C.Handshake.Generator;
using C2C.Medium;
using C2C.Message.Encoder;
using C2C.Processor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace C2C
{
    public class C2Channel : IDisposable
    {
        public bool CanReceive => medium.CanReceive;
        public bool CanTransmit => medium.CanTransmit;

        public Guid ChannelId { get; }
        public Guid SessionId { get; }

        public event EventHandler<HandshakeEventArgs> OnHandshake;
        public event EventHandler<DataEventArgs> OnReceive;

        private readonly IMedium medium;
        private readonly IMessageEncoder encoder;
        private readonly IHandshakeGenerator handshakeGenerator;
        private readonly IHandshakeEncoder handshakeEncoder;

        private IProcessor[] processors;
        private IProcessor[] processorsReversed;

        private bool enableProcessors = false;
        private bool disposedValue;

        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>> receiveHandlerQueue
            = new ConcurrentDictionary<Guid, TaskCompletionSource<byte[]>>();

        public C2Channel(Guid channelId, Guid sessionId, IMedium medium, IMessageEncoder encoder, IHandshakeGenerator handshakeGenerator, IHandshakeEncoder handshakeEncoder, IProcessor[] processors)
        {
            SessionId = sessionId;
            this.medium = medium;
            this.encoder = encoder;
            this.handshakeGenerator = handshakeGenerator;
            this.handshakeEncoder = handshakeEncoder;
            this.processors = processors;

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

            if (packet.SessionID != SessionId) // Ignore other session packet
                return;

            // Process asynchronously
            Task.Run(() =>
            {
                using (var sha256 = SHA256.Create())
                {
                    var hash = sha256.ComputeHash(packet.Data);
                    if (hash != packet.DataHash)
                        return; // Drop corrupted or tampered data
                }

                var data = packet.Data;

                if (enableProcessors && processorsReversed != null)
                {
                    foreach (var processor in processorsReversed) // We must apply processors in reversed order to properly process.
                        data = processor.ProcessIncomingData(data);
                }

                if (receiveHandlerQueue.TryRemove(packet.MessageID, out var taskCompletion))
                    taskCompletion.SetResult(data);

                OnReceive(this, new DataEventArgs(data));
            });
        }

        public void Transmit(Guid messageId, byte[] data)
        {
            Task.Run(() =>
            {
                if (enableProcessors)
                {
                    foreach (var processor in processors)
                        data = processor.ProcessOutgoingData(data);
                }

                var packetBytes = encoder.Encode(SessionId, messageId, data);
                medium.Transmit(packetBytes);
            });
        }

        public async Task<byte[]> Transceive(Guid messageId, byte[] buffer, TimeSpan timeout)
        {
            Transmit(messageId, buffer);
            return await WaitForResponse(messageId, timeout);
        }

        public async Task<byte[]> WaitForResponse(Guid messageId, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            // Enqueue the response wait
            receiveHandlerQueue[messageId] = tcs;

            // Wait until the response received
            return await Task.Run(() =>
            {
                tcs.Task.Wait(timeout);
                return tcs.Task.Result;
            });
        }

        public async Task Open(TimeSpan timeout)
        {
            medium.Open(timeout);

            // handshake & negotiation
            var handshakeRequest = handshakeGenerator.Generate(ChannelId, processors);
            var handshakeRequestBytes = handshakeEncoder.Encode(handshakeRequest);
            var handshakeResponseBytes = await Transceive(Guid.NewGuid(), handshakeRequestBytes, timeout);
            OnHandshake(this, new HandshakeEventArgs(SessionId, handshakeResponseBytes));

            var handshakeResponse = handshakeEncoder.Decode(handshakeResponseBytes);
            if (handshakeResponse.ChannelId != ChannelId)
                throw new IOException("Unsupported channel id");

            // Only enable processors that are supported in both sides.
            var succeedNegotiations = handshakeResponse.ProcessorNegotiations.Intersect(handshakeRequest.ProcessorNegotiations).ToArray();

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
