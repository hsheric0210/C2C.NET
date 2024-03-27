using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace C2C
{
    internal static class MessageDataConvert
    {
        /// <summary>
        /// Decodes the raw data byte-array just received from the network to managed <c>MessageData</c> struct.
        /// </summary>
        /// <param name="data">The raw data byte-array.</param>
        /// <returns>Managed <c>MessageData</c> struct of the given data.</returns>
        /// <exception cref="InvalidDataException">Thrown when the data hash mismatches. (Corrupted or tampered)</exception>
        public static MessageData FromRaw(byte[] data)
        {
            var uidBuffer = new byte[16];
            Buffer.BlockCopy(data, 0, uidBuffer, 0, 16);

            var hashBuffer = new byte[32];
            Buffer.BlockCopy(data, 16, hashBuffer, 0, 32);

            var dataBuffer = new byte[data.Length - 48];
            Buffer.BlockCopy(data, 48, dataBuffer, 0, dataBuffer.Length);

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(data);
                if (!hash.SequenceEqual(hashBuffer))
                    throw new InvalidDataException("Corrupted data");
            }

            return new MessageData(new Guid(uidBuffer), hashBuffer, data.Length, data);
        }

        /// <summary>
        /// Encodes the given data with unique-id to a raw data byte-array that can be sent through the network.
        /// </summary>
        /// <param name="uid">The unique id of the data transmission.</param>
        /// <param name="data">The data to send through the network.</param>
        /// <returns></returns>
        public static byte[] ToRaw(Guid uid, byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(uid.ToByteArray(), 0, 16);

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(data);
                stream.Write(hash, 0, sha256.HashSize);
            }

            stream.Write(BitConverter.GetBytes(data.Length), 0, sizeof(int));
            stream.Write(data, 0, data.Length);
            return stream.ToArray();
        }
    }
}
