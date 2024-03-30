using System;
using System.IO;
using System.Security.Cryptography;

namespace C2C.Message.Encoder
{
    public class C2MessageEncoder : IMessageEncoder
    {
        /// <inheritdoc/>
        public int GetPacketSize(int dataLength) => dataLength + 68;

        /// <inheritdoc/>
        public byte[] Encode(Guid sessionId, Guid messageId, byte[] data)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {

                writer.Write(sessionId.ToByteArray());
                writer.Write(messageId.ToByteArray());

                using (var sha256 = SHA256.Create())
                {
                    var hash = sha256.ComputeHash(data);
                    writer.Write((short)sha256.HashSize);
                    writer.Write(hash);
                }

                writer.Write(data.Length);
                writer.Write(data);
                return stream.ToArray();
            }
        }

        /// <inheritdoc/>
        public IMessagePacket Decode(byte[] data)
        {
            using (var stream = new MemoryStream(data))
                return Decode(stream);
        }

        /// <inheritdoc/>
        public IMessagePacket Decode(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var sessionId = new Guid(reader.ReadBytes(16));
                var messageId = new Guid(reader.ReadBytes(16));

                var dataHashSize = reader.ReadInt16();
                var dataHash = reader.ReadBytes(dataHashSize);

                var dataLength = reader.ReadInt32();
                var data = reader.ReadBytes(dataLength);

                return new C2MessagePacket(sessionId, messageId, dataHash, data);
            }
        }
    }
}
