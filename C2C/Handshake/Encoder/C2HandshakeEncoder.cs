using System;
using System.IO;
using C2C.Processor;

namespace C2C.Handshake.Encoder
{
    public class C2HandshakeEncoder : IHandshakeEncoder
    {
        public byte[] Encode(Guid channelId, IProcessor[] processors)
        {
            var negotiations = new C2Handshake.ProcessorNegotiation[processors.Length];
            for (var i = 0; i < processors.Length; i++)
            {
                var processor = processors[i];
                negotiations[i] = new C2Handshake.ProcessorNegotiation(processor.ProcessorID, processor.GetNegotationData());
            }
            return Encode(new C2Handshake(channelId, negotiations));
        }

        public byte[] Encode(IHandshake handshake)
        {
            using (var stream = new MemoryStream(16384))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(handshake.ChannelId.ToByteArray());

                var negotiationCount = handshake.ProcessorNegotiations.Length;
                writer.Write(negotiationCount);

                for (var i = 0; i < negotiationCount; i++)
                {
                    var negotiation = handshake.ProcessorNegotiations[i];
                    writer.Write(negotiation.ProcessorId.ToByteArray());
                    writer.Write(negotiation.ProcessorParameter.Length);
                    writer.Write(negotiation.ProcessorParameter);
                }

                return stream.ToArray();
            }
        }

        public IHandshake Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream))
            {
                var channelId = new Guid(reader.ReadBytes(16));

                var negotiationCount = reader.ReadInt32();

                var negotiations = new C2Handshake.ProcessorNegotiation[negotiationCount];

                for (var i = 0; i < negotiationCount; i++)
                {
                    var processorId = new Guid(reader.ReadBytes(16));

                    var processorParameterLength = reader.ReadInt32();
                    var processorParameter = reader.ReadBytes(processorParameterLength);

                    negotiations[i] = new C2Handshake.ProcessorNegotiation(processorId, processorParameter);
                }

                return new C2Handshake(channelId, negotiations);
            }
        }
    }
}
