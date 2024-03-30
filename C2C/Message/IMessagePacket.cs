using System;

namespace C2C.Message
{
    public interface IMessagePacket
    {
        Guid SessionID { get; }

        Guid MessageID { get; }

        byte[] DataHash { get; }

        byte[] Data { get; }
    }
}
