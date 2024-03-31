using System;

namespace C2C
{
    public class DataEventArgs : EventArgs
    {
        public DataEventArgs(Guid messageId, byte[] data)
        {
            MessageID = messageId;
            Data = data;
        }

        public Guid MessageID { get; }
        public byte[] Data { get; }
    }
}
