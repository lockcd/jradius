using JRadius.Core.Packet.Attribute;
using System;
using System.IO;

namespace JRadius.Core.Packet
{
    public class DiameterFormat : Format
    {
        private const byte AVP_VENDOR = 0x80;

        public override void PackAttribute(MemoryStream buffer, RadiusAttribute a)
        {
            var attributeValue = a.GetValue();
            int length = attributeValue.GetLength();
            int padding = ((length + 0x03) & ~0x03) - length;
            PackHeader(buffer, a);
            attributeValue.GetBytes(buffer);
            while (padding-- > 0)
            {
                PutUnsignedByte(buffer, 0);
            }
        }

        public void PackHeader(MemoryStream buffer, RadiusAttribute a)
        {
            if (a is VSAttribute vsa)
            {
                PackHeader(buffer, vsa);
                return;
            }

            var attributeValue = a.GetValue();
            PutUnsignedInt(buffer, a.GetType());
            PutUnsignedByte(buffer, 0);
            PutUnsignedByte(buffer, 0); // part of the AVP Length!
            PutUnsignedShort(buffer, attributeValue.GetLength() + 8);
        }

        public void PackHeader(MemoryStream buffer, VSAttribute a)
        {
            var attributeValue = a.GetValue();
            PutUnsignedInt(buffer, a.GetVsaAttributeType());
            PutUnsignedByte(buffer, AVP_VENDOR);
            PutUnsignedByte(buffer, 0); // part of the AVP Length!
            PutUnsignedShort(buffer, attributeValue.GetLength() + 12);
            PutUnsignedInt(buffer, a.GetVendorId());
        }

        public override void UnpackAttributeHeader(MemoryStream buffer, AttributeParseContext ctx)
        {
            ctx.AttributeType = (int)ReadUnsignedInt(buffer);
            int flags = ReadUnsignedByte(buffer);
            ReadUnsignedByte(buffer);
            ctx.AttributeLength = ReadUnsignedShort(buffer);
            ctx.HeaderLength = 8;

            if ((flags & AVP_VENDOR) > 0)
            {
                ctx.VendorNumber = (int)ReadUnsignedInt(buffer);
                ctx.HeaderLength += 4;
            }

            ctx.Padding = (int)(((ctx.AttributeLength + 0x03) & ~0x03) - ctx.AttributeLength);
        }
    }
}
