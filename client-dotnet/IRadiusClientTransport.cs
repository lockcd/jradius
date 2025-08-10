using JRadius.Core.Packet;
using System.Net;

namespace JRadius.Core.Client
{
    public interface IRadiusClientTransport
    {
        void SetRadiusClient(RadiusClient client);
        RadiusResponse SendReceive(RadiusRequest p, int retries);
        void Send(RadiusRequest p, int attempt);
        void Close();
        int GetAcctPort();
        void SetAcctPort(int acctPort);
        int GetAuthPort();
        void SetAuthPort(int authPort);
        int GetSocketTimeout();
        void SetSocketTimeout(int socketTimeout);
        IPAddress GetRemoteInetAddress();
        void SetRemoteInetAddress(IPAddress remoteInetAddress);
        IPAddress GetLocalInetAddress();
        void SetLocalInetAddress(IPAddress localInetAddress);
        string GetSharedSecret();
        void SetSharedSecret(string sharedSecret);
    }
}
