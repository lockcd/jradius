using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace JRadius.Core.Client
{
    public class RadiusMultiClient : RadiusClient
    {
        private IMemoryCache _requestCache;

        public RadiusMultiClient(IMemoryCache requestCache)
            : base()
        {
            _requestCache = requestCache;
        }

        public RadiusMultiClient(IMemoryCache requestCache, UdpClient socket, IPAddress address, string secret, int authPort, int acctPort, int timeout)
            : base(socket, address, secret, authPort, acctPort, timeout)
        {
            _requestCache = requestCache;
        }

        public RadiusMultiClient(IMemoryCache requestCache, UdpClient socket, IPAddress address, string secret)
            : base(socket, address, secret)
        {
            _requestCache = requestCache;
        }

        public RadiusMultiClient(IMemoryCache requestCache, IPAddress address, string secret, int authPort, int acctPort, int timeout)
            : base(address, secret, authPort, acctPort, timeout)
        {
            _requestCache = requestCache;
        }

        public RadiusMultiClient(IMemoryCache requestCache, IPAddress address, string secret)
            : base(address, secret)
        {
            _requestCache = requestCache;
        }

        public RadiusMultiClient(IMemoryCache requestCache, IRadiusClientTransport transport)
            : base(transport)
        {
            _requestCache = requestCache;
        }
    }
}
