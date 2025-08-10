using JRadius.Core.Packet.Attribute;
using System;
using System.Collections.Generic;
using System.IO;

namespace JRadius.Core.Packet
{
    public class RadiusFormat : Format
    {
        private const int HEADER_LENGTH = 2;
        public const int VSA_HEADER_LENGTH = 8;

        private static readonly RadiusFormat _staticFormat = new RadiusFormat();

        public static RadiusFormat GetInstance()
        {
            return _staticFormat;
        }

        public static void SetAttributeBytes(RadiusPacket packet, MemoryStream buffer, int length)
        {
            _staticFormat.UnpackAttributes(packet.GetAttributes(), buffer, length, packet.IsRecyclable());
        }

        public void PackPacket(RadiusPacket packet, string sharedSecret, MemoryStream buffer, bool onWire)
        {
            if (packet == null)
            {
                throw new ArgumentException("Packet is null.");
            }

            var initialPosition = buffer.Position;
            buffer.Position = initialPosition + RadiusPacket.RADIUS_HEADER_LENGTH;
            PackAttributeList(packet.GetAttributes(), buffer, onWire);

            var totalLength = buffer.Position - initialPosition;
            var attributesLength = totalLength - RadiusPacket.RADIUS_HEADER_LENGTH;

            try
            {
                buffer.Position = initialPosition;
                PackHeader(buffer, packet, buffer.ToArray(), (int)initialPosition + RadiusPacket.RADIUS_HEADER_LENGTH, (int)attributesLength, sharedSecret);
                buffer.Position = totalLength + initialPosition;
            }
            catch (Exception e)
            {
                // TODO: Log warning
            }
        }

        public void PackHeader(MemoryStream buffer, RadiusPacket p, byte[] attributeBytes, int offset, int attributesLength, string sharedSecret)
        {
            var length = attributesLength + RadiusPacket.RADIUS_HEADER_LENGTH;
            PutUnsignedByte(buffer, p.GetCode());
            PutUnsignedByte(buffer, p.GetIdentifier());
            PutUnsignedShort(buffer, length);
            buffer.Write(p.CreateAuthenticator(attributeBytes, offset, attributesLength, sharedSecret));
        }

        public override void PackAttribute(MemoryStream buffer, RadiusAttribute a)
        {
            var attributeValue = a.GetValue();
            if (a is VSAttribute vsa)
            {
                // TODO: Implement VSA packing
            }
            else
            {
                PackHeader(buffer, a);
                attributeValue.GetBytes(buffer);
            }
        }

        public void PackHeader(MemoryStream buffer, RadiusAttribute a)
        {
            PackHeader(buffer, a, a.GetValue().GetLength());
        }

        public void PackHeader(MemoryStream buffer, RadiusAttribute a, int valueLength)
        {
            if (a is VSAttribute vsa)
            {
                // TODO: Implement VSA header packing
            }
            else
            {
                PutUnsignedByte(buffer, (int)a.GetType());
                PutUnsignedByte(buffer, valueLength + HEADER_LENGTH);
            }
        }

        public override void UnpackAttributeHeader(MemoryStream buffer, AttributeParseContext ctx)
        {
            ctx.AttributeOp = 0;
            ctx.VendorNumber = -1;
            ctx.Padding = 0;
            ctx.AttributeType = GetUnsignedByte(buffer);
            ctx.AttributeLength = GetUnsignedByte(buffer);
            ctx.HeaderLength = 2;
        }
    }
}
