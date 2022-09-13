using System.Net;
using System.Net.Sockets;

namespace Dora.OpenTelemetry.Zipkin
{
    internal class LocalEndpointResolver : ILocalEndpointResolver
    {        
        public ZipkinEndpoint Resolve(string serviceName)
        {
            var hostName = ResolveHostName();

            string? ipv4 = null;
            string? ipv6 = null;
            if (!string.IsNullOrEmpty(hostName))
            {
                ipv4 = ResolveHostAddress(hostName, AddressFamily.InterNetwork);
                ipv6 = ResolveHostAddress(hostName, AddressFamily.InterNetworkV6);
            }

            return new ZipkinEndpoint(
                serviceName,
                ipv4,
                ipv6,
                port: null);
        }

        private static string? ResolveHostName()
        {
            string? result = null;

            try
            {
                result = Dns.GetHostName();

                if (!string.IsNullOrEmpty(result))
                {
                    var response = Dns.GetHostEntry(result);

                    if (response != null)
                    {
                        return response.HostName;
                    }
                }
            }
            catch { }
            return result;
        }

        private static string? ResolveHostAddress(string hostName, AddressFamily family)
        {
            string? result = null;

            try
            {
                var results = Dns.GetHostAddresses(hostName);

                if (results != null && results.Length > 0)
                {
                    foreach (var addr in results)
                    {
                        if (addr.AddressFamily.Equals(family))
                        {
                            var sanitizedAddress = new IPAddress(addr.GetAddressBytes()); // Construct address sans ScopeID
                            result = sanitizedAddress.ToString();

                            break;
                        }
                    }
                }
            }
            catch { }
            return result;
        }
    }
}
