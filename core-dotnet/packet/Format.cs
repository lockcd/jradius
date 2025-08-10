using JRadius.Core.Packet.Attribute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JRadius.Core.Packet
{
    public abstract class Format
    {
        public abstract void PackAttribute(MemoryStream buffer, RadiusAttribute a);

        public void PackAttributes(MemoryStream buffer, List<VSAttribute> list)
        {
            foreach (var a in list)
            {
                PackAttribute(buffer, a);
            }
        }

        public abstract void UnpackAttributeHeader(MemoryStream buffer, AttributeParseContext ctx);

        public void PackAttributeList(AttributeList attrs, MemoryStream buffer, bool onWire)
        {
            var iterator = attrs.GetAttributeList().GetEnumerator();
            while (iterator.MoveNext())
            {
                var attr = iterator.Current;
                // TODO: Implement the rest of the logic
            }
        }

        protected class AttributeParseContext
        {
            public long AttributeType = 0;
            public long AttributeLength = 0;
            public long AttributeOp = RadiusAttribute.Operator.EQ;
            public long AttributeValueLength = 0;
            public byte[] AttributeValue = null;
            public int HeaderLength = 0;
            public int VendorNumber = -1;
            public int Padding = 0;
            public long LengthRemaining = 0;
        }

        public void UnpackAttributes(AttributeList attrs, MemoryStream buffer, int length, bool pool)
        {
            var ctx = new AttributeParseContext();
            int pos = 0;
            while (pos < length)
            {
                try
                {
                    UnpackAttributeHeader(buffer, ctx);
                }
                catch (Exception e)
                {
                    // TODO: Log error
                    return;
                }
                // TODO: Implement the rest of the logic
            }
        }

        public static long ReadUnsignedInt(Stream input)
        {
            var b = new byte[4];
            input.Read(b, 0, 4);
            return (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
        }

        public static int ReadUnsignedShort(Stream input)
        {
            var b = new byte[2];
            input.Read(b, 0, 2);
            return (b[0] << 8) | b[1];
        }

        public static int ReadUnsignedByte(Stream input)
        {
            return input.ReadByte() & 0xFF;
        }

        public static void WriteUnsignedByte(Stream output, int b)
        {
            output.WriteByte((byte)b);
        }

        public static void WriteUnsignedShort(Stream output, int s)
        {
            output.WriteByte((byte)(s >> 8));
            output.WriteByte((byte)s);
        }

        public static void WriteUnsignedInt(Stream output, long i)
        {
            WriteUnsignedShort(output, (int)(i >> 16));
            WriteUnsignedShort(output, (int)i);
        }

        public static short GetUnsignedByte(MemoryStream bb)
        {
            return (short)(bb.ReadByte() & 0xff);
        }

        public static void PutUnsignedByte(MemoryStream bb, int value)
        {
            bb.WriteByte((byte)(value & 0xff));
        }

        public static int GetUnsignedShort(MemoryStream bb)
        {
            return (short)((bb.ReadByte() << 8) | bb.ReadByte());
        }

        public static void PutUnsignedShort(MemoryStream bb, int value)
        {
            bb.WriteByte((byte)(value >> 8));
            bb.WriteByte((byte)value);
        }

        public static long GetUnsignedInt(MemoryStream bb)
        {
            return (uint)((bb.ReadByte() << 24) | (bb.ReadByte() << 16) | (bb.ReadByte() << 8) | bb.ReadByte());
        }

        public static void PutUnsignedInt(MemoryStream bb, long value)
        {
            bb.WriteByte((byte)(value >> 24));
            bb.WriteByte((byte)(value >> 16));
            bb.WriteByte((byte)(value >> 8));
            bb.WriteByte((byte)value);
        }
    }
}
