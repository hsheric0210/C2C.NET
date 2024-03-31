using C2C;
using C2C.Handshake;
using C2C.Handshake.Encoder;
using C2C.Handshake.Generator;
using C2C.Medium;
using C2C.Medium.Http;
using C2C.Medium.Tcp;
using C2C.Message.Encoder;
using C2C.Processor;
using C2C.Processor.Compression;
using C2C.Processor.Encryption;
using C2C.Processor.Encryption.Scheme;
using CommandLine;
using System.Net;

namespace ExampleApplication
{
    internal class Program
    {
        class Options
        {
            [Option('t', "type", Required = true, Default = "tcp_bind", HelpText = "Communication medium type; supported media: tcp_bind, tcp_connect, http_bind, http_connect, ws_bind, ws_connect")]
            public string MediumType { get; set; }

            [Option('c', "channel-id", Required = false, Default = "320DF748-D5BC-4778-988A-86E92C2E60C8", HelpText = "Communication channel ID.")]
            public string ChannelID { get; set; }

            [Option('m', "message-id", Required = false, Default = "0AFEA770-519F-478D-A6CE-E3CBEEE8B6F4", HelpText = "Message ID to use when communicating with each other.")]
            public string MessageID { get; set; }

            [Option('a', "address", Required = true, HelpText = "Address to communicate with.")]
            public string Address { get; set; }
        }

        private static Guid commMessageId;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsedAsync(Execute).Wait();
        }

        private async static Task Execute(Options options)
        {
            if (!Guid.TryParse(options.ChannelID, out var channelId))
                throw new ArgumentException("Specified Channel ID is not a valid GUID");

            if (!Guid.TryParse(options.MessageID, out commMessageId))
                throw new ArgumentException("Specified Message ID is not a valid GUID");

            IMedium medium;
            switch (options.MediumType.ToLowerInvariant())
            {
                case "tcp_bind":
                {
                    Console.WriteLine("[+] Using TCP Bind mode.");
                    if (!IPEndPoint.TryParse(options.Address, out var address))
                        throw new ArgumentException(options.Address + " is not a valid ip endpoint");
                    medium = new TcpBind(address);
                    break;
                }
                case "tcp_connect":
                {
                    Console.WriteLine("[+] Using TCP Connect mode.");
                    if (!IPEndPoint.TryParse(options.Address, out var address))
                        throw new ArgumentException(options.Address + " is not a valid ip endpoint");
                    medium = new TcpConnect(address);
                    break;
                }
                case "http_bind":
                {
                    Console.WriteLine("[+] Using HTTP Bind mode.");
                    medium = new HttpBind(new string[] { options.Address });
                    break;
                }
                case "http_connect":
                {
                    Console.WriteLine("[+] Using HTTP Connect mode.");
                    if (!Uri.TryCreate(options.Address, new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = false }, out var uri))
                        throw new ArgumentException(options.Address + " is not a valid URI");
                    medium = new HttpConnect(uri, TimeSpan.FromSeconds(3));
                    break;
                }
                case "ws_bind":
                {
                    Console.WriteLine("[+] Using WebSocket Bind mode.");
                    medium = new WebSocketBind(new string[] { options.Address });
                    break;
                }
                case "ws_connect":
                {
                    Console.WriteLine("[+] Using WebSocket Connect mode.");
                    if (!Uri.TryCreate(options.Address, new UriCreationOptions { DangerousDisablePathAndQueryCanonicalization = false }, out var uri))
                        throw new ArgumentException(options.Address + " is not a valid URI");
                    medium = new WebSocketConnect(uri);
                    break;
                }
                default:
                    throw new ArgumentException("Unsupported medium type: " + options.MediumType);
            }

            var processors = new IProcessor[2];
            processors[0] = new GZipCompressionProcessor();
            processors[1] = new EphemeralEncryptionProcessorCng(new AesScheme());
            var channel = new C2Channel(channelId, medium, new C2MessageEncoder(), new C2HandshakeGenerator(), new C2HandshakeEncoder(), processors);
            channel.OnReceive += Channel_OnReceive;

            Console.WriteLine("[+] Waiting for the channel to open...");
            await channel.Open();
            Console.WriteLine("[+] Connected! (Type '!exit' to end session)");

            while (true)
            {
                var command = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(command))
                    continue;

                if (command.Equals("!exit"))
                {
                    Console.WriteLine("[+] Closing the session...");
                    break;
                }

                if (!channel.Connected)
                {
                    Console.WriteLine("[-] Connection lost.");
                    break;
                }

                var timeStamp = DateTime.Now;
                var data = MessagePacketEncoder.Encode(timeStamp, command);
                channel.Transmit(commMessageId, data);
                Console.WriteLine("sent@{0} : {1}", timeStamp, command);
            }

            channel.OnReceive -= Channel_OnReceive;
            medium.Dispose();
        }

        private static void Channel_OnReceive(object? sender, DataEventArgs e)
        {
            if (e.MessageID != commMessageId)
                return;

            try
            {
                (var timeStamp, var message) = MessagePacketEncoder.Decode(e.Data);
                Console.WriteLine("recv@{0} : {1}", timeStamp, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception decoding the received message." + ex.ToString());
            }
        }
    }
}
