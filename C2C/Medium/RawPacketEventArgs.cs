using System;

namespace C2C.Medium
{
    public class RawPacketEventArgs : EventArgs
    {
        public RawPacketEventArgs(byte[] rawPacket) => RawPacket = rawPacket;

        public byte[] RawPacket { get; }
    }
}
