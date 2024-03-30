using System;

namespace C2C
{
    public class DataEventArgs : EventArgs
    {
        public DataEventArgs(byte[] data) => Data = data;

        public byte[] Data { get; }
    }

    public class HandshakeEventArgs : EventArgs
    {
        public HandshakeEventArgs(Guid sessionId, byte[] data)
        {
            SessionId = sessionId;
            Data = data;
        }

        public Guid SessionId { get; }
        public byte[] Data { get; }
    }
}
