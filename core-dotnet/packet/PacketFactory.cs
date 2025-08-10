using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace JRadius.Core.Packet
{
    public static class PacketFactory
    {
        private static readonly Dictionary<int, Type> _codeMap = new Dictionary<int, Type>();

        static PacketFactory()
        {
            _codeMap[0] = typeof(NullPacket);
            _codeMap[AccessRequest.CODE] = typeof(AccessRequest);
            _codeMap[AccessAccept.CODE] = typeof(AccessAccept);
            _codeMap[AccessReject.CODE] = typeof(AccessReject);
            _codeMap[AccountingRequest.CODE] = typeof(AccountingRequest);
            _codeMap[AccountingResponse.CODE] = typeof(AccountingResponse);
            // TODO: Add other packet types
        }

        public static RadiusPacket Parse(UdpReceiveResult datagram, bool pool)
        {
            var buffer = new MemoryStream(datagram.Buffer);
            RadiusPacket rp = null;
            try
            {
                rp = ParseUDP(buffer, pool);
            }
            catch (System.Exception e)
            {
                // TODO: Log error
            }
            return rp;
        }

        public static RadiusPacket ParseUDP(MemoryStream buffer, bool pool)
        {
            var code = Format.ReadUnsignedByte(buffer);
            var identifier = Format.ReadUnsignedByte(buffer);
            var length = Format.ReadUnsignedShort(buffer);
            return ParseUDP(code, identifier, length, buffer, pool);
        }

        public static RadiusPacket ParseUDP(int code, int identifier, int length, MemoryStream buffer, bool pool)
        {
            RadiusPacket rp = null;
            if (_codeMap.TryGetValue(code, out var type))
            {
                rp = (RadiusPacket)Activator.CreateInstance(type);
            }
            else
            {
                throw new System.Exception($"bad radius code - {code}");
            }

            var bAuthenticator = new byte[16];
            buffer.Read(bAuthenticator, 0, 16);
            rp.SetIdentifier(identifier);
            rp.SetAuthenticator(bAuthenticator);

            length -= RadiusPacket.RADIUS_HEADER_LENGTH;
            if (length > 0)
            {
                RadiusFormat.SetAttributeBytes(rp, buffer, length);
            }

            return rp;
        }
    }
}
