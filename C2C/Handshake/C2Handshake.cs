using System;

namespace C2C.Handshake
{
    public class C2Handshake : IHandshake
    {
        public Guid ChannelId { get; }
        public ProcessorNegotiation[] ProcessorNegotiations { get; }

        public C2Handshake(Guid channelId, ProcessorNegotiation[] processorNegotiations)
        {
            ChannelId = channelId;
            ProcessorNegotiations = processorNegotiations;
        }

        public readonly struct ProcessorNegotiation : IEquatable<ProcessorNegotiation>
        {
            public Guid ProcessorId { get; }
            public byte[] ProcessorParameter { get; }

            public ProcessorNegotiation(Guid processorId, byte[] processorParameter)
            {
                ProcessorId = processorId;
                ProcessorParameter = processorParameter;
            }

            public bool Equals(ProcessorNegotiation other) => ProcessorId == other.ProcessorId;
        }
    }
}
