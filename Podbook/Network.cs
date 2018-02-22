using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Hiale.Podbook
{
    public class Network
    {
        public static string GetPublicIpv4()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var response = client.DownloadString("http://checkip.dyndns.org");
                    var ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                    var match = ip.Match(response);
                    return match.Value;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IEnumerable<string> GetAllLocalIPv4()
        {
            var addresses = new List<string>();
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;
                var ipProperties = networkInterface.GetIPProperties();
                var gatewayAddress = ipProperties.GatewayAddresses.FirstOrDefault();
                if (gatewayAddress == null || Equals(gatewayAddress.Address, IPAddress.Any))
                    continue;
                foreach (var ip in ipProperties.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        addresses.Add(ip.Address.ToString());
                    }
                }
            }
            return addresses;
        }

        public static bool PortInUse(int port)
        {
            var ipEndPoints = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            return ipEndPoints.Any(endPoint => endPoint.Port == port);
        }
    }
}
