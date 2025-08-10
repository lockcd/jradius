using JRadius.Core.Packet;
using JRadius.Core.Util;
using System;
using System.Net;
using System.Net.Sockets;

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
        protected ITransportStatusListener _statusListener;

        protected abstract void Send(RadiusRequest req, int attempt);
        protected abstract RadiusResponse Receive(RadiusRequest req);

        public abstract void Close();

        public RadiusResponse SendReceive(RadiusRequest p, int retries)
        {
            RadiusResponse r = null;
            int tries = 0;

            if (p is AccessRequest)
            {
                try
                {
                    MessageAuthenticator.GenerateRequestMessageAuthenticator(p, SharedSecret);
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Error generating message authenticator", e);
                }
            }

            if (retries < 0) retries = 0;
            retries++; // do at least one

            while (tries < retries)
            {
                try
                {
                    Send(p, tries);
                    r = Receive(p);
                    break;
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        // TODO: Log warning
                    }
                    else
                    {
                        // TODO: Log error
                    }
                }
                catch (System.Exception e)
                {
                    // TODO: Log error
                }
                tries++;
            }

            if (tries == retries)
            {
                throw new TimeoutException("Timeout: No Response from RADIUS Server");
            }

            if (!r.VerifyAuthenticator(p.GetAuthenticator(), SharedSecret))
            {
                throw new System.Security.SecurityException("Invalid RADIUS Authenticator");
            }

            if (!MessageAuthenticator.VerifyReply(p.GetAuthenticator(), r, SharedSecret))
            {
                throw new System.Security.SecurityException("Invalid RADIUS Message-Authenticator");
            }

            return r;
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

        public void SetStatusListener(ITransportStatusListener statusListener)
        {
            _statusListener = statusListener;
        }

        void IRadiusClientTransport.Send(RadiusRequest p, int attempt)
        {
            Send(p, attempt);
        }
    }
}
