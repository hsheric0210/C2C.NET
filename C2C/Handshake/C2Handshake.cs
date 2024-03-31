using System;

namespace C2C.Handshake
{
    public partial class C2Handshake : IHandshake
    {
        public Guid ChannelId { get; }
        public ProcessorNegotiation[] ProcessorNegotiations { get; }

        public C2Handshake(Guid channelId, ProcessorNegotiation[] processorNegotiations)
        {
            ChannelId = channelId;
            ProcessorNegotiations = processorNegotiations;
        }
    }
}
