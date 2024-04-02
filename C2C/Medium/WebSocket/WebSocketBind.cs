using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace C2C.Medium.Http
{
    // Inspired by https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7

    public class WebSocketBind : IMedium
    {
        private HttpListener listener;
        private WebSocket socket;
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;

        public Guid MediumID => Guid.Parse("7524EAC5-7666-4178-A5C8-B2EC3F61EA85");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public bool Connected => true;

        private readonly IList<byte[]> pendingTransmitRequests = new List<byte[]>();
        private readonly TimeSpan connectionCheckPeriod;
        private readonly TimeSpan readPeriod;

        public WebSocketBind(string[] prefixes, TimeSpan connectionCheckPeriod, TimeSpan readPeriod)
        {
            listener = new HttpListener();
            foreach (var prefix in prefixes)
                listener.Prefixes.Add(prefix);

            cancelToken = new CancellationTokenSource();
            this.connectionCheckPeriod = connectionCheckPeriod;
            this.readPeriod = readPeriod;
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public Task Open()
        {
            listener.Start();

            // Connection Accept Thread
            return Task.Run(async () =>
            {
                var context = await listener.GetContextAsync();
                if (!context.Request.IsWebSocketRequest)
                {
                    // Ignore all requests except WebSocket upgrade request
                    context.Response.StatusCode = 418;
                    context.Response.Close();
                }

                var webSocketContext = await context.AcceptWebSocketAsync("null");
                socket = webSocketContext.WebSocket;

                _ = Task.Run(async () =>
                {
                    Logging.Log("Reader thread started.");

                    // Wait for connection
                    while (socket.State == WebSocketState.Connecting && !cancelToken.IsCancellationRequested)
                        await Task.Delay(connectionCheckPeriod);

                    Logging.Log("WebSocket connected.");

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

                        await Task.Delay(readPeriod); // Wait 10ms before reading next data
                    }

                });
            });
        }

        public void Transmit(byte[] rawPacket)
        {
            if (socket == null || socket.State != WebSocketState.Open)
                return;

            Logging.Log("Transmit {0} bytes", rawPacket.Length);
            Task.Run(async () => await socket.SendAsync(new ArraySegment<byte>(rawPacket), WebSocketMessageType.Binary, true, cancelToken.Token));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancelToken.Cancel();
                    listener.Stop();
                }

                listener = null;
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
