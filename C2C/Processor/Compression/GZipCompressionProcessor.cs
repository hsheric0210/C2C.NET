using System;
using System.IO;
using System.IO.Compression;

namespace C2C.Processor.Compression
{
    /// <summary>
    /// Compresses the data with GZip algorithm.
    /// Not recommended unless the payload data is large enough and mostly composed of texts.
    /// </summary>
    public class GZipCompressionProcessor : IProcessor
    {
        public Guid ProcessorID => Guid.Parse("AF3BB5A5-FE08-4F64-90B0-00506B0DB7A1");

        public byte[] GetNegotationData() => Array.Empty<byte>();

        public void FinishNegotiate(byte[] negotiationData) { }

        public byte[] ProcessIncomingData(byte[] data)
        {
            using (var outStream = new MemoryStream())
            using (var inStream = new MemoryStream(data))
            using (var gzStream = new GZipStream(inStream, CompressionMode.Decompress))
            {
                gzStream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }

        public byte[] ProcessOutgoingData(byte[] data)
        {
            using (var outStream = new MemoryStream())
            using (var gzStream = new GZipStream(outStream, CompressionLevel.Optimal))
            {
                gzStream.Write(data, 0, data.Length);
                gzStream.Flush();
                return outStream.ToArray();
            }
        }
    }
}
