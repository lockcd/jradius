using JRadius.Core.Packet;
using System;
using System.IO;
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
            RemoteInetAddress = ((IPEndPoint)_socket.Client.RemoteEndPoint).Address;
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

            var buffer = new MemoryStream(4096);
            _format.PackPacket(req, SharedSecret, buffer, true);
            _socket.Send(buffer.ToArray(), (int)buffer.Position, new IPEndPoint(RemoteInetAddress, port));
        }

        protected override RadiusResponse Receive(RadiusRequest req)
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var replyBytes = _socket.Receive(ref remoteEP);
            var replyPacket = PacketFactory.Parse(new UdpReceiveResult(replyBytes, remoteEP), req.IsRecyclable());

            if (!(replyPacket is RadiusResponse))
            {
                throw new System.Exception("Received something other than a RADIUS Response to a Request");
            }

            return (RadiusResponse)replyPacket;
        }

        public override void SetSocketTimeout(int timeout)
        {
            base.SetSocketTimeout(timeout);
            _socket.Client.ReceiveTimeout = SocketTimeout;
        }
    }
}
