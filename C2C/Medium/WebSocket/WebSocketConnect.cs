using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace C2C.Medium.Tcp
{
    public class WebSocketConnect : IMedium
    {
        private ClientWebSocket socket;
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;
        private readonly Uri address;

        public Guid MediumID => Guid.Parse("60C0C6A8-9066-417A-BFF4-9D8AFECC2D93");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public bool Connected => socket.State == WebSocketState.Open;

        public WebSocketConnect(Uri address)
        {
            this.address = address;

            socket = new ClientWebSocket();
            socket.Options.AddSubProtocol("null");
            cancelToken = new CancellationTokenSource();
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public async Task Open()
        {
            Logging.Log("Begin connect.");
            await socket.ConnectAsync(address, cancelToken.Token);

            _ = Task.Run(async () =>
            {
                while (!Connected && !cancelToken.IsCancellationRequested)
                    Task.Delay(100).Wait(); // Wait 100ms to be connected

                while (!cancelToken.IsCancellationRequested)
                {
                    if (socket.State > WebSocketState.Open) // closing
                    {
                        Logging.Log("WebSocket connection lost.");
                        break;
                    }

                    var data = new List<byte>(65536);
                    var endOfData = false;
                    do
                    {
                        var readBuffer = new byte[32768];
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(readBuffer), cancelToken.Token);
                        endOfData = result.EndOfMessage;
                        data.AddRange(readBuffer);
                        Logging.Log("Received {0} bytes.", result.Count);
                    } while (!endOfData);

                    OnReceive(this, new RawPacketEventArgs(data.ToArray()));
                }
            });
        }

        public void Transmit(byte[] buffer)
        {
            if (!Connected)
                return;

            Logging.Log("Transmit {0} bytes.", buffer.Length);
            Task.Run(async () => await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, cancelToken.Token));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancelToken.Cancel();
                    socket.Dispose();
                }

                socket = null;
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
