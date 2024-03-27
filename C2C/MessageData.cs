using System;

namespace C2C
{
    public readonly struct MessageData
    {
        internal MessageData(Guid uniqueId, byte[] hash, int length, byte[] data)
        {
            UniqueId = uniqueId;
            Hash = hash;
            Length = length;
            Data = data;
        }

        public Guid UniqueId { get; }
        public byte[] Hash { get; }
        public int Length { get; }
        public byte[] Data { get; }
    }
}
