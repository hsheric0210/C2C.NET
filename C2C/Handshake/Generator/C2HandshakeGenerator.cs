using C2C.Processor;
using System;

namespace C2C.Handshake.Generator
{
    public class C2HandshakeGenerator : IHandshakeGenerator
    {
        public IHandshake Generate(Guid channelId, IProcessor[] processors)
        {
            var negotiations = new C2Handshake.ProcessorNegotiation[processors.Length];
            for (var i = 0; i < processors.Length; i++)
                negotiations[i] = new C2Handshake.ProcessorNegotiation(processors[i].ProcessorID, processors[i].GetNegotationData());

            return new C2Handshake(channelId, negotiations);
        }
    }
}
