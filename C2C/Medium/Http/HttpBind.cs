using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace C2C.Medium.Http
{
    // Inspired by https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7

    public class HttpBind : IMedium
    {
        private HttpListener listener;
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;

        public Guid MediumID => Guid.Parse("5A7BDE0E-EA9D-422E-8200-2B9E18EC966F");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public bool Connected => true;

        private readonly IList<byte[]> pendingTransmitRequests = new List<byte[]>();

        public HttpBind(string[] prefixes)
        {
            listener = new HttpListener();
            foreach (var prefix in prefixes)
                listener.Prefixes.Add(prefix);

            cancelToken = new CancellationTokenSource();
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public Task Open()
        {
            listener.Start();

            // Connection Accept Thread
            Task.Run(() =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;

                    if (request.HttpMethod == "POST") // Data transmit request
                    {
                        Logging.Log("Pending data transmit request accepted.");

                        using (var stream = new MemoryStream())
                        {
                            request.InputStream.CopyTo(stream);
                            OnReceive(this, new RawPacketEventArgs(stream.ToArray())); // todo: additional obfuscation or encoding when communicating through HTTP request body
                        }

                        response.StatusCode = 204;
                    }
                    else if (request.HttpMethod == "GET")
                    {
                        Logging.Log("Pending data polling request accepted.");

                        // header: <int32: # of data blocks (N)><int32: data block 1 size><int32: data block 2 size> ... <int32: data block N size>
                        // body: <byte[]: data block 1><byte[]: data block 2><byte[]: data block 3> ... <byte[]: data block N>

                        // Dump all pending data transmit requests to the client
                        using (var writer = new BinaryWriter(response.OutputStream))
                        {
                            // header
                            var transmitCount = pendingTransmitRequests.Count;
                            writer.Write(transmitCount);
                            foreach (var transmit in pendingTransmitRequests)
                                writer.Write(transmit.Length);

                            // body
                            foreach (var transmit in pendingTransmitRequests)
                                writer.Write(transmit);
                        }

                        pendingTransmitRequests.Clear(); // clear the pending queue
                    }

                    response.Close();
                }
            });

            return Task.CompletedTask;
        }

        public void Transmit(byte[] rawPacket)
        {
            Logging.Log("Enqueued transmit data request: {0} bytes", rawPacket.Length);
            pendingTransmitRequests.Add(rawPacket);
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
