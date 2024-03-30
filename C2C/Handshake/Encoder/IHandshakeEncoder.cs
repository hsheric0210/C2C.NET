namespace C2C.Handshake.Encoder
{
    public interface IHandshakeEncoder
    {
        byte[] Encode(IHandshake handshake);

        IHandshake Decode(byte[] bytes);
    }
}