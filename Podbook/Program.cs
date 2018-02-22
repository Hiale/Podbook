using System;
using System.Diagnostics;
using System.Reflection;

namespace Hiale.Podbook
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Console.WriteLine($"Podbook v{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}, {FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).LegalCopyright}");
                Console.WriteLine();
                Console.WriteLine("Press CTRL+C to close this application");
                Console.WriteLine();
                var context = new PodbookContext(new ArgumentData(args));
                if (context.Initialize())
                {
                    context.Start();
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Usage:");
                    Console.WriteLine("podbook [-path (directory which contains the episodes)] [-host (host name or IP of this machine)] [-port (HTTP server port)]");
                    Console.WriteLine();
                    Console.WriteLine("All parameters can be omitted, in this case, the program asks those interactively.");
                    Console.WriteLine("path\tDirectory which contains the episodes.");
                    Console.WriteLine("host\tHost name or IP of this machine.");
                    Console.WriteLine("post\tPort, which should be used to connect to this program.");
                    Console.WriteLine();
                    Console.WriteLine("Examples:");
                    Console.WriteLine(@"podbook -path C:\audiobooks\1983");
                    Console.WriteLine(@"podbook -path ""H:\my Audiobooks\Journey"" -port 1234");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error:{ex.Message}");
            }
        }
    }
}
