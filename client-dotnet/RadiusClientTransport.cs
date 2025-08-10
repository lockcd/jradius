using JRadius.Core.Packet;
using System.Net;

namespace JRadius.Core.Client
{
    public abstract class RadiusClientTransport : IRadiusClientTransport
    {
        protected IPAddress LocalInetAddress;
        protected IPAddress RemoteInetAddress;
        protected string SharedSecret;

        protected int AuthPort;
        protected int AcctPort;

        public const int DefaultTimeout = 60;
        protected int SocketTimeout = DefaultTimeout * 1000;

        protected RadiusClient _radiusClient;

        protected abstract void Send(RadiusRequest req, int attempt);
        protected abstract RadiusResponse Receive(RadiusRequest req);

        public abstract void Close();

        public RadiusResponse SendReceive(RadiusRequest p, int retries)
        {
            // TODO: Implement this method
            return null;
        }

        public int GetAcctPort()
        {
            return AcctPort;
        }

        public void SetAcctPort(int acctPort)
        {
            AcctPort = acctPort;
        }

        public int GetAuthPort()
        {
            return AuthPort;
        }

        public void SetAuthPort(int authPort)
        {
            AuthPort = authPort;
        }

        public int GetSocketTimeout()
        {
            return SocketTimeout / 1000;
        }

        public virtual void SetSocketTimeout(int socketTimeout)
        {
            SocketTimeout = socketTimeout * 1000;
        }

        public IPAddress GetRemoteInetAddress()
        {
            return RemoteInetAddress;
        }

        public void SetRemoteInetAddress(IPAddress remoteInetAddress)
        {
            RemoteInetAddress = remoteInetAddress;
        }

        public IPAddress GetLocalInetAddress()
        {
            return LocalInetAddress;
        }

        public void SetLocalInetAddress(IPAddress localInetAddress)
        {
            LocalInetAddress = localInetAddress;
        }

        public string GetSharedSecret()
        {
            return SharedSecret;
        }

        public void SetSharedSecret(string sharedSecret)
        {
            SharedSecret = sharedSecret;
        }

        public void SetRadiusClient(RadiusClient client)
        {
            _radiusClient = client;
        }
    }
}
