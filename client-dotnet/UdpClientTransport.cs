using JRadius.Core.Packet;
using System;
using System.Net;
using System.Net.Sockets;

namespace JRadius.Core.Client
{
    public class UdpClientTransport : RadiusClientTransport
    {
        private static readonly RadiusFormat _format = RadiusFormat.GetInstance();
        public const int DefaultAuthPort = 1812;
        public const int DefaultAcctPort = 1813;

        private UdpClient _socket;

        public UdpClientTransport()
        {
            _socket = new UdpClient();
        }

        public UdpClientTransport(UdpClient socket)
        {
            _socket = socket;
        }

        public override void Close()
        {
            _socket?.Close();
        }

        protected override void Send(RadiusRequest req, int attempt)
        {
            var port = req is AccountingRequest ? AcctPort : AuthPort;
            if (attempt > 1)
            {
                // TODO: Log retry
            }

            // TODO: Implement packet packing
            var buffer = new byte[4096];
            _socket.Send(buffer, buffer.Length, new IPEndPoint(RemoteInetAddress, port));
        }

        protected override RadiusResponse Receive(RadiusRequest req)
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var replyBytes = _socket.Receive(ref remoteEP);
            // TODO: Implement packet parsing
            return null;
        }

        public override void SetSocketTimeout(int timeout)
        {
            base.SetSocketTimeout(timeout);
            _socket.Client.ReceiveTimeout = SocketTimeout;
        }
    }
}
