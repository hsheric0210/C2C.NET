using System;

namespace C2C.Handshake
{
    public interface IHandshake
    {
        Guid ChannelId { get; }
        C2Handshake.ProcessorNegotiation[] ProcessorNegotiations { get; }
    }
}