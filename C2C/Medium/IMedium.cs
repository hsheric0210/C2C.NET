using System;

namespace C2C.Medium
{
    public interface IMedium : IDisposable
    {
        Guid MediumID { get; }

        bool CanReceive { get; }

        bool CanTransmit { get; }

        event EventHandler<RawPacketEventArgs> OnReceive;

        void Open(TimeSpan timeout);

        void Transmit(byte[] buffer);
    }
}
