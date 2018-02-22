using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using Hiale.Podbook.Feed;
using Hiale.Podbook.httpServer;

namespace Hiale.Podbook
{
    public class PodbookContext
    {
        private readonly ArgumentData _argumentData;
        private CustomHttpServer _httpServer;
        private ManualResetEvent _resetEvent;
        private string _feedContent;
        private readonly Dictionary<string, string> _files;

        public PodbookContext(ArgumentData argumentData)
        {
            _argumentData = argumentData;
            _files = new Dictionary<string, string>();
        }

        public bool Initialize()
        {
            _resetEvent = new ManualResetEvent(false);
            return !_argumentData.HelpMode;
        }

        public void Start()
        {
            CheckPath();
            CheckHost();
            CheckPort();

            StartWebServer();

            _resetEvent.WaitOne();
            Console.WriteLine();
            Console.WriteLine("Server running!");
            Console.WriteLine("Add following URL to your podcast application:");
            Console.WriteLine($"http://{_argumentData.Host}:{_argumentData.Port}");
            Console.WriteLine("You can also try this URL in your mobile browser to check whether you can connect to this computer.");
            Console.WriteLine("Download all desired episodes and press a key to shut down the server afterwards.");

            Console.ReadLine();
            _httpServer.Stop();
            Console.WriteLine("Server stopped.");
        }

        private void Cancel()
        {
            _httpServer?.Stop();
            throw new OperationCanceledException();
        }

        private void CheckPath()
        {
            if (!Directory.Exists(_argumentData.Path))
            {
                while (true)
                {
                    Console.WriteLine("Enter directory which contains the episides and press Enter:");
                    var path = Console.ReadLine();
                    if (path == null)
                        Cancel();
                    if (Directory.Exists(path))
                    {
                        _argumentData.Path = path;
                        break;
                    }

                    Console.WriteLine("Invalid directory.");
                }
            }

            Console.WriteLine($"Using directory: {_argumentData.Path}");
            Console.WriteLine();
        }

        private void CheckHost()
        {
            if (string.IsNullOrEmpty(_argumentData.Host))
            {
                var options = new Dictionary<string, Tuple<string, string>>();
                var index = 1;
                try
                {
                    options.Add(index.ToString(), new Tuple<string, string>(Dns.GetHostName(), "host name"));
                    index++;
                }
                catch (Exception)
                {
                    //ignore
                }

                var publicIp = Network.GetPublicIpv4();
                if (!string.IsNullOrEmpty(publicIp))
                {
                    options.Add(index.ToString(), new Tuple<string, string>(publicIp, "public IP address"));
                    index++;
                }

                var localIps = Network.GetAllLocalIPv4();
                foreach (var localIp in localIps)
                {
                    options.Add(index.ToString(), new Tuple<string, string>(localIp, "local IP address"));
                    index++;
                }

                options.Add(index.ToString(), new Tuple<string, string>("<Enter custom host name or IP address>", null));


                while (true)
                {
                    Console.WriteLine("Select a host name or IP address which can be used to connect to this machine and press Enter:");
                    Console.WriteLine();
                    foreach (var option in options)
                    {
                        Console.WriteLine($"{option.Key}\t{option.Value.Item1}{(string.IsNullOrEmpty(option.Value.Item2) ? string.Empty : $" ({option.Value.Item2})")}");
                    }

                    var optionKey = Console.ReadLine();
                    if (optionKey == null)
                        Cancel();
                    if (options.TryGetValue(optionKey, out var optionValue))
                    {
                        string host;
                        if (optionValue.Item2 == null) //custom
                        {
                            Console.WriteLine("Enter custom host name or IP address and press Enter:");
                            host = Console.ReadLine();
                            if (host == null)
                                Cancel();
                            if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
                            {
                                Console.WriteLine("Unvalid host name or IP address");
                                continue;
                            }
                        }
                        else
                        {
                            host = optionValue.Item1;
                        }

                        _argumentData.Host = host;
                        break;
                    }

                    Console.WriteLine("Invalid option.");
                }
            }

            Console.WriteLine($"Using host: {_argumentData.Host}");
            Console.WriteLine();
        }

        private void CheckPort()
        {
            if (_argumentData.Port < 1)
                return;
            if (!Network.PortInUse(_argumentData.Port))
                return;
            while (true)
            {
                Console.WriteLine($"Port {_argumentData.Port} already in use.");
                Console.WriteLine("Enter a port number or 0 to automatically use an empty port and press Enter:");
                var portStr = Console.ReadLine();
                if (portStr == null)
                    Cancel();
                if (int.TryParse(portStr, out var port))
                {
                    if (!Network.PortInUse(port))
                        break;
                    Console.WriteLine("Port already in use.");
                    continue;
                }
                Console.WriteLine("Invalid port number.");
            }
        }

        private string GenerateFeed()
        {
            var channel = new Channel {Link = $"http://{_argumentData.Host}:{_argumentData.Port}"};
            var rss = new Rss {Channel = channel};
            _files.Clear();
            var files = Directory.GetFiles(_argumentData.Path, "*.mp3");
            for (var i = 0; i < files.Length; i++)
            {
                var file = new FileInfo(files[i]);
                var safeFilename = GetSafeFilename(Path.GetFileName(file.FullName));
                _files.Add(safeFilename, file.FullName);
                var episode = new Episode($"http://{_argumentData.Host}:{_argumentData.Port}/files/{safeFilename}");
                var tagFile = TagLib.File.Create(file.FullName);
                if (tagFile?.Tag != null)
                {
                    var tag = tagFile.Tag;
                    if (i == 0)
                    {
                        channel.Title = tag.Album ?? string.Empty;
                        channel.Description = tag.Comment ?? string.Empty;
                    }
                    episode.Title = tag.Title ?? string.Empty;
                    episode.Description = tag.Comment ?? string.Empty;
                    episode.Track = tag.Track;
                }
                else
                {
                    if (i == 0)
                    {
                        channel.Title = Path.GetFileName(Path.GetDirectoryName(file.FullName));
                    }
                    episode.Title = Path.GetFileNameWithoutExtension(file.FullName);
                }
                episode.Timestamp = file.LastWriteTimeUtc;
                episode.Length = file.Length;
                channel.Episodes.Add(episode);
            }

            string feedContent;
            using (var writer = new Utf8StringWriter())
            {
                var serializer = new XmlSerializer(typeof(Rss));
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                serializer.Serialize(writer, rss, namespaces);
                feedContent = writer.ToString();
            }
            return feedContent;
        }

        private void StartWebServer()
        {
            var routeConfig = new List<CustomRoute>
            {
                new CustomRoute
                {
                    Name = "XML Feed Handler",
                    UrlRegex = @"^/$",
                    Method = HttpMethods.GET,
                    Callable = request =>
                    {
                        var response = HttpResponseExtension.Ok;
                        response.ContentAsUTF8 = _feedContent;
                        return response;
                    }
                },
                new CustomRoute
                {
                    Name = "FileSystem Handler",
                    UrlRegex = @"^/files/(.*)$",
                    Method = HttpMethods.GET,
                    Callable = new CustomFileSystemRouteHandler(_files).Handle
                },
                new CustomRoute
                {
                    Name = "Favicon",
                    UrlRegex = @"^/favicon.ico$",
                    Method = HttpMethods.GET,
                    Callable = request => HttpResponseExtension.NotFound
                }
            };
            if (_argumentData.Port == -1)
                _argumentData.Port = 80;


            _httpServer = new CustomHttpServer(_argumentData.Port, routeConfig);
            _httpServer.PortObtained += (sender, args) =>
            {
                _argumentData.Port = args.Port;
                Console.WriteLine($"Using port: {_argumentData.Port}");
                _feedContent = GenerateFeed();
                _resetEvent.Set();
            };
            _httpServer.StartListening();
        }

        private static string GetSafeFilename(string input)
        {
            return Regex.Replace(input, @"[^A-Za-z0-9_.~-]", string.Empty);
        }
    }
}
