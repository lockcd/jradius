using JRadius.Core.Client.Auth;
using JRadius.Core.Packet;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace JRadius.Core.Client
{
    public class RadiusClient
    {
        protected IRadiusClientTransport _transport;
        protected static readonly Dictionary<string, Type> _authenticators = new Dictionary<string, Type>();

        static RadiusClient()
        {
            // TODO: Register authenticators
        }

        public RadiusClient()
        {
            _transport = new UdpClientTransport();
            _transport.SetRadiusClient(this);
        }

        public RadiusClient(UdpClient socket)
        {
            _transport = new UdpClientTransport(socket);
            _transport.SetRadiusClient(this);
        }

        public RadiusClient(IRadiusClientTransport transport)
        {
            _transport = transport;
            _transport.SetRadiusClient(this);
        }

        public RadiusClient(IPAddress address, string secret)
            : this()
        {
            SetRemoteInetAddress(address);
            SetSharedSecret(secret);
        }

        public RadiusClient(UdpClient socket, IPAddress address, string secret)
            : this(socket)
        {
            SetRemoteInetAddress(address);
            SetSharedSecret(secret);
        }

        public RadiusClient(IPAddress address, string secret, int authPort, int acctPort, int timeout)
            : this()
        {
            SetRemoteInetAddress(address);
            SetSharedSecret(secret);
            SetAuthPort(authPort);
            SetAcctPort(acctPort);
            SetSocketTimeout(timeout);
        }

        public RadiusClient(UdpClient socket, IPAddress address, string secret, int authPort, int acctPort, int timeout)
            : this(socket)
        {
            SetRemoteInetAddress(address);
            SetSharedSecret(secret);
            SetAuthPort(authPort);
            SetAcctPort(acctPort);
            SetSocketTimeout(timeout);
        }

        public void Close()
        {
            _transport?.Close();
        }

        public static void RegisterAuthenticator(string name, Type c)
        {
            _authenticators[name] = c;
        }

        public static void RegisterAuthenticator(string name, string className)
        {
            var c = Type.GetType(className);
            _authenticators[name] = c;
        }

        public int GetAcctPort()
        {
            return _transport.GetAcctPort();
        }

        public void SetAcctPort(int acctPort)
        {
            _transport.SetAcctPort(acctPort);
        }

        public int GetAuthPort()
        {
            return _transport.GetAuthPort();
        }

        public void SetAuthPort(int authPort)
        {
            _transport.SetAuthPort(authPort);
        }

        public int GetSocketTimeout()
        {
            return _transport.GetSocketTimeout();
        }

        public void SetSocketTimeout(int socketTimeout)
        {
            _transport.SetSocketTimeout(socketTimeout);
        }

        public IPAddress GetRemoteInetAddress()
        {
            return _transport.GetRemoteInetAddress();
        }

        public void SetRemoteInetAddress(IPAddress remoteInetAddress)
        {
            _transport.SetRemoteInetAddress(remoteInetAddress);
        }

        public IPAddress GetLocalInetAddress()
        {
            return _transport.GetLocalInetAddress();
        }

        public void SetLocalInetAddress(IPAddress localInetAddress)
        {
            // TODO: create a socket bound to the localInetAddress
        }

        public string GetSharedSecret()
        {
            return _transport.GetSharedSecret();
        }

        public void SetSharedSecret(string sharedSecret)
        {
            _transport.SetSharedSecret(sharedSecret);
        }
    }
}
