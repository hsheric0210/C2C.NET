using System;
using System.Threading.Tasks;

namespace C2C.Medium
{
    public interface IMedium : IDisposable
    {
        Guid MediumID { get; }

        bool CanReceive { get; }

        bool CanTransmit { get; }

        bool Connected { get; }

        event EventHandler<RawPacketEventArgs> OnReceive;

        Task Open();

        void Transmit(byte[] buffer);
    }
}
