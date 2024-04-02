using System;
using System.Threading.Tasks;

namespace C2C.Medium
{
    public interface IMedium : IDisposable
    {
        /// <summary>
        /// The unique identifier of this medium.
        /// </summary>
        Guid MediumID { get; }

        /// <summary>
        /// Is this medium supports receiving messages?
        /// </summary>
        bool CanReceive { get; }

        /// <summary>
        /// Is this medium supports transmitting messages?
        /// </summary>
        bool CanTransmit { get; }

        /// <summary>
        /// Is this medium properly connected to the endpoint\?
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Fired when a raw message packet received.
        /// </summary>
        event EventHandler<RawPacketEventArgs> OnReceive;

        /// <summary>
        /// Open the connection. When the connection is successfully opened, the returned <c>Task</c> will be marked as complete.
        /// </summary>
        /// <returns>The task. Completed when the connection is successfully established.</returns>
        Task Open();

        /// <summary>
        /// Transmit the raw message packet.
        /// </summary>
        /// <param name="rawPacket">The message packet data.</param>
        void Transmit(byte[] rawPacket);
    }
}
