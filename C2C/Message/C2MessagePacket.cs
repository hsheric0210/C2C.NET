using System;

namespace C2C.Message
{
    public class C2MessagePacket : IMessagePacket
    {
        public Guid SessionID { get; }
        public Guid MessageID { get; }
        public byte[] DataHash { get; }
        public byte[] Data { get; }

        public C2MessagePacket(Guid sessionId, Guid messageId, byte[] dataHash, byte[] data)
        {
            SessionID = sessionId;
            MessageID = messageId;
            DataHash = dataHash;
            Data = data;
        }
    }
}
