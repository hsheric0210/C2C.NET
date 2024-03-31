using System;

namespace C2C.Handshake
{
    public partial class C2Handshake
    {
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
