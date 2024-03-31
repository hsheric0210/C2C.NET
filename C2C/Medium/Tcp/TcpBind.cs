using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace C2C.Medium.Tcp
{
    public class TcpBind : IMedium
    {
        private TcpListener listener;
        private TcpClient client;
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;
        private readonly int readBufferSize;

        public Guid MediumID => Guid.Parse("76613C01-640A-4550-88A7-DA174BF21163");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public bool Connected => client?.Connected == true;

        public TcpBind(IPEndPoint address, int readBufferSize)
        {
            listener = new TcpListener(address);
            cancelToken = new CancellationTokenSource();
            this.readBufferSize = readBufferSize;
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public Task Open()
        {
            listener.Start();

            // Connection Accept Thread
            return Task.Run(() =>
            {
                while (!listener.Pending())
                    Task.Delay(100).Wait(); // Wait 100ms before retrying

                client = listener.AcceptTcpClient();
                Logging.Log("Pending connection accepted.");

                // Socket Reader Thread
                Task.Run(() =>
                {
                    Logging.Log("Reader thread started.");
                    var stream = client.GetStream();
                    while (!cancelToken.IsCancellationRequested)
                    {
                        if (!client.Connected)
                        {
                            Logging.Log("Client connection lost.");
                            break;
                        }

                        while (stream.DataAvailable)
                        {
                            var available = client.Available;
                            var readBuffer = new byte[available];
                            var read = stream.Read(readBuffer, 0, available);
                            Logging.Log("Received {0} bytes out of {1} available bytes.", read, available);

                            OnReceive(this, new RawPacketEventArgs(readBuffer));
                        }

                        Task.Delay(100).Wait(); // Wait 10ms before reading next data
                    }
                });
            });
        }

        public void Transmit(byte[] buffer)
        {
            Logging.Log("Transmit data: {0} bytes", buffer.Length);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancelToken.Cancel();
                    listener.Stop();
                    client.Dispose();
                }

                listener = null;
                client = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
