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
        private TcpClient client;
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;
        private readonly TimeSpan connectionAcceptPeriod;
        private readonly TimeSpan readPeriod;

        public Guid MediumID => Guid.Parse("76613C01-640A-4550-88A7-DA174BF21163");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public bool Connected => client?.Connected == true;

        public TcpBind(IPEndPoint address, TimeSpan connectionAcceptPeriod, TimeSpan readPeriod)
        {
            listener = new TcpListener(address);
            cancelToken = new CancellationTokenSource();
            this.connectionAcceptPeriod = connectionAcceptPeriod;
            this.readPeriod = readPeriod;
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public Task Open()
        {
            listener.Start();

            // Connection Accept Thread
            return Task.Run(() =>
            {
                while (!listener.Pending())
                    Task.Delay(connectionAcceptPeriod).Wait();

                client = listener.AcceptTcpClient();
                Logging.Log("Pending connection accepted.");

                // Start the socket reader thread
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

                        Task.Delay(readPeriod).Wait();
                    }
                });
            });
        }

        public void Transmit(byte[] rawPacket)
        {
            Logging.Log("Transmit data: {0} bytes", rawPacket.Length);
            client.GetStream().Write(rawPacket, 0, rawPacket.Length);
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
