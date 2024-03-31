namespace ExampleApplication
{
    internal static class MessagePacketEncoder
    {
        public static byte[] Encode(DateTime timeStamp, string message)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(timeStamp.ToBinary());
                writer.Write(message);
                return stream.ToArray();
            }
        }

        public static (DateTime, string) Decode(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var timeStamp = DateTime.FromBinary(reader.ReadInt64());
                var message = reader.ReadString();
                return (timeStamp, message);
            }
        }
    }
}
