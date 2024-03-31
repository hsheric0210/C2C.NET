using System;
using System.Threading.Tasks;

namespace C2C
{
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// Can this channel able to receive the message from the server?
        /// </summary>
        bool CanReceive { get; }

        /// <summary>
        /// Can this channel able to transmit the message to the server?
        /// </summary>
        bool CanTransmit { get; }

        bool Connected { get; }

        /// <summary>
        /// Channel ID of this channel instance.
        /// </summary>
        Guid ChannelId { get; }


        /// <summary>
        /// Called when the incoming handshake packet is received.
        /// </summary>
        event EventHandler<DataEventArgs> OnHandshake;

        /// <summary>
        /// Called when a message is received from the server.
        /// </summary>
        event EventHandler<DataEventArgs> OnReceive;

        /// <summary>
        /// Wait for the specific message with specified unique identifier.
        /// </summary>
        /// <param name="messageId">The message unique identifier to wait for.</param>
        /// <param name="timeout">Timeout to wait for the server response.</param>
        /// <returns></returns>
        Task<byte[]> WaitForResponse(Guid messageId, TimeSpan timeout);

        /// <summary>
        /// Transmits a message to the server.
        /// </summary>
        /// <param name="messageId">Unique identifier for this message.</param>
        /// <param name="data">The message data.</param>
        void Transmit(Guid messageId, byte[] data);

        /// <summary>
        /// Transmits a request message to the server, then wait for the response.
        /// </summary>
        /// <param name="messageId">Unique identifier for this message transaction.</param>
        /// <param name="data">The request message data.</param>
        /// <param name="timeout">Timeout to wait for the server response.</param>
        /// <returns></returns>
        Task<byte[]> Transceive(Guid messageId, byte[] data, TimeSpan timeout);

        /// <summary>
        /// Opens the communication channel.
        /// </summary>
        /// <param name="timeout">Timeout to wait for the server.</param>
        /// <exception cref="IOException">Thrown when the channel ID is differ from server's one.</exception>
        Task Open();
    }
}