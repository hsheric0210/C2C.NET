using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace C2C.Medium.Tcp
{
    public class TcpConnect : IMedium
    {
        private TcpClient client;
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;
        private readonly IPEndPoint address;
        private readonly int readBufferSize;

        public Guid MediumID => Guid.Parse("B7DFA60A-3A80-4914-9434-E61C5DD6DA31");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public bool Connected => client.Connected;

        public TcpConnect(IPEndPoint address, int readBufferSize)
        {
            client = new TcpClient();
            cancelToken = new CancellationTokenSource();
            this.address = address;
            this.readBufferSize = readBufferSize;
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public Task Open()
        {
            Logging.Log("Begin connect.");
            var task = client.ConnectAsync(address.Address, address.Port);

            Task.Run(() =>
            {
                while (!client.Connected && !cancelToken.IsCancellationRequested)
                {
                    Task.Delay(100).Wait(); // Wait 100ms to be connected
                }

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

            return task;
        }

        public void Transmit(byte[] buffer)
        {
            if (!client.Connected)
                return;

            Logging.Log("Sent {0} bytes.", buffer.Length);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Close();
                    cancelToken.Cancel();
                    client.Dispose();
                }

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
