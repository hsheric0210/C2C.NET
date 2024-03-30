using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace C2C.Medium.Tcp
{
    public class TcpBind : IMedium
    {
        private TcpListener listener;
        private HashSet<TcpClient> clients = new HashSet<TcpClient>();
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;
        private readonly int readBufferSize;

        public Guid MediumID => Guid.Parse("76613C01-640A-4550-88A7-DA174BF21163");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public TcpBind(IPAddress ip, int port, int readBufferSize)
        {
            listener = new TcpListener(ip, port);
            cancelToken = new CancellationTokenSource();
            this.readBufferSize = readBufferSize;
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public void Open(TimeSpan timeout)
        {
            listener.Start();

            // Connection Accept Thread
            Task.Run(() =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    if (!listener.Pending())
                    {
                        Task.Delay(100); // Wait 100ms before retrying
                        continue;
                    }

                    var client = listener.AcceptTcpClient();
                    clients.Add(client);

                    // Socket Reader Thread
                    Task.Run(() =>
                    {
                        var stream = client.GetStream();
                        while (!cancelToken.IsCancellationRequested)
                        {
                            if (!stream.DataAvailable)
                                continue;

                            var readBuffer = new byte[readBufferSize];
                            stream.Read(readBuffer, 0, readBufferSize);
                            OnReceive(this, new RawPacketEventArgs(readBuffer));

                            while (stream.DataAvailable) // More data is available (the packet size exceeds the buffer size -> cannot handle; drop)
                                stream.Read(readBuffer, 0, readBufferSize);
                        }
                    });
                }
            });
        }

        public void Transmit(byte[] buffer)
        {
            foreach (var client in clients)
            {
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancelToken.Cancel();
                    listener.Stop();
                    foreach (var client in clients)
                        client.Dispose();
                }

                listener = null;
                clients = null;
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
