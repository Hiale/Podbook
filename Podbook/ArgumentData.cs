using System;
using System.Collections.Generic;
using System.IO;

namespace Hiale.Podbook
{
    public class ArgumentData
    {
        public bool HelpMode { get; set; }

        public int Port { get; set; }

        public string Path { get; set; }

        public string Host { get; set; }

        public ArgumentData(IList<string> args)
        {
            var arguments = ParseCommandLine(args);
            HelpMode = IsHelp(arguments);
            Port = GetPort(arguments) ?? -1;
            Path = GetPath(arguments);
            Host = GetHost(arguments);
        }

        private static bool IsHelp(IReadOnlyDictionary<string, string> arguments)
        {
            return arguments.ContainsKey("help");
        }

        private static int? GetPort(IReadOnlyDictionary<string, string> arguments)
        {
            if (!arguments.TryGetValue("port", out var portString))
                return null;
            return int.TryParse(portString, out var port) ? (int?)port : null;
        }

        private static string GetPath(IReadOnlyDictionary<string, string> arguments)
        {
            if (!arguments.TryGetValue("path", out var path))
                return null;
            var directoryInfo = new DirectoryInfo(path);
            return directoryInfo.Exists ? directoryInfo.FullName : null;
        }

        private static string GetHost(IReadOnlyDictionary<string, string> arguments)
        {
            if (!arguments.TryGetValue("host", out var ip))
                return null;
            return Uri.CheckHostName(ip) != UriHostNameType.Unknown ? ip : null;
        }

        private static Dictionary<string, string> ParseCommandLine(IList<string> args)
        {
            var arguments = new Dictionary<string, string>();
            if (args.Count == 1)
            {
                var arg = args[0].ToLower();
                if (arg == "-help" || arg == "-?")
                {
                    arguments.Add("help", null);
                    return arguments;
                }
            }
            string currentName = null;
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    currentName = arg.Substring(1).ToLower();
                    arguments[currentName] = null;
                }
                else if (!string.IsNullOrEmpty(currentName))
                    arguments[currentName] = arg;
            }
            return arguments;
        }
    }
}
