using System.Net.Sockets;

namespace C2C.Protocols.Tcp
{
    public class TcpReceiver : IReceiver
    {
        public TcpListener listener;

        public TcpReceiver(int port)
        {
            listener = new TcpListener(port);
        }
    }
}
