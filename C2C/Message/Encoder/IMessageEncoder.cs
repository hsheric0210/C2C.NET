using System;
using System.IO;

namespace C2C.Message.Encoder
{
    public interface IMessageEncoder
    {
        /// <summary>
        /// Returns the expected length of the packet when specified data is encoded.
        /// </summary>
        /// <param name="dataLength">The raw data size.</param>
        /// <returns>Expected packet size.</returns>
        int GetPacketSize(int dataLength);

        /// <summary>
        /// Encodes the given data to a message packet.
        /// </summary>
        /// <param name="sessionId">The client UUID.</param>
        /// <param name="sessionId">The message UUID.</param>
        /// <param name="data">The data/payload.</param>
        /// <returns></returns>
        byte[] Encode(Guid sessionId, Guid messageId, byte[] data);

        /// <summary>
        /// Decodes the raw data byte-array just received from the network to managed <c>MessageData</c> struct.
        /// </summary>
        /// <param name="data">The raw data byte-array.</param>
        /// <returns>Managed <c>MessageData</c> struct of the given data.</returns>
        /// <exception cref="InvalidDataException">Thrown when the data hash mismatches. (Corrupted or tampered)</exception>
        IMessagePacket Decode(byte[] data);

        /// <summary>
        /// Reads and decodes a message packet from stream.
        /// </summary>
        /// <remarks>
        /// This function does not perform the message hash check.
        /// </remarks>
        /// <param name="data">The data steram to read a message packet from.</param>
        /// <returns>Decoded message packet.</returns>
        IMessagePacket Decode(Stream stream);
    }
}