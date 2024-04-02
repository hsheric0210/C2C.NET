using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace C2C.Medium.Http
{
    public class HttpConnect : IMedium
    {
        private HttpClient client;
        private bool disposedValue;

        private readonly CancellationTokenSource cancelToken;
        private readonly Uri address;
        private readonly TimeSpan pollingDelay;

        public Guid MediumID => Guid.Parse("E7D0EB93-3100-4E88-B2F3-2A3D547D8C3D");

        public bool CanReceive => true;

        public bool CanTransmit => true;

        public bool Connected => true;

        public HttpConnect(Uri address, TimeSpan pollingDelay)
        {
            this.address = address;
            this.pollingDelay = pollingDelay;

            client = new HttpClient
            {
                BaseAddress = address
            };

            cancelToken = new CancellationTokenSource();
        }

        public event EventHandler<RawPacketEventArgs> OnReceive;

        public Task Open()
        {
            Logging.Log("Begin connect.");

            Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var pollingResponse = await client.GetAsync(address);
                        if (!pollingResponse.IsSuccessStatusCode)
                            continue;

                        using (var reader = new BinaryReader(await pollingResponse.Content.ReadAsStreamAsync()))
                        {
                            var blockCount = reader.ReadInt32();
                            var blockSizes = new int[blockCount];
                            for (var i = 0; i < blockCount; i++)
                                blockSizes[i] = reader.ReadInt32();

                            for (var i = 0; i < blockCount; i++)
                            {
                                var data = reader.ReadBytes(blockSizes[i]);
                                OnReceive(this, new RawPacketEventArgs(data));
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Logging.Log("Polling request exception. {0}", ex.ToString());
                    }

                    Task.Delay(pollingDelay).Wait();
                }
            });

            return Task.CompletedTask;
        }

        public void Transmit(byte[] rawPacket)
        {
            Logging.Log("Sent {0} bytes.", rawPacket.Length);

            var content = new ByteArrayContent(rawPacket);
            Task.Run(async () => await client.PostAsync(address, content));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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
