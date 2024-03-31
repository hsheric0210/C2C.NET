using System;

namespace C2C.Message
{
    public class C2MessagePacket : IMessagePacket
    {
        public Guid MessageID { get; }
        public byte[] DataHash { get; }
        public byte[] Data { get; }

        public C2MessagePacket(Guid messageId, byte[] dataHash, byte[] data)
        {
            MessageID = messageId;
            DataHash = dataHash;
            Data = data;
        }
    }
}
