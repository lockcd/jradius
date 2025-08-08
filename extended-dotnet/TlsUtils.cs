using System;
using System.IO;
using System.Security.Cryptography;

namespace JRadius.Extended.Tls
{
    public class TlsUtils
    {
        public static short ReadUint8(Stream stream)
        {
            int i = stream.ReadByte();
            if (i == -1)
                throw new EndOfStreamException();
            return (short)i;
        }

        public static int ReadUint16(Stream stream)
        {
            int i1 = stream.ReadByte();
            int i2 = stream.ReadByte();
            if (i2 == -1)
                throw new EndOfStreamException();
            return (i1 << 8) | i2;
        }

        public static int ReadUint24(Stream stream)
        {
            int i1 = stream.ReadByte();
            int i2 = stream.ReadByte();
            int i3 = stream.ReadByte();
            if (i3 == -1)
                throw new EndOfStreamException();
            return (i1 << 16) | (i2 << 8) | i3;
        }

        public static void WriteUint8(short i, Stream stream)
        {
            stream.WriteByte((byte)i);
        }

        public static void WriteUint16(int i, Stream stream)
        {
            stream.WriteByte((byte)(i >> 8));
            stream.WriteByte((byte)i);
        }

        public static void WriteUint24(int i, Stream stream)
        {
            stream.WriteByte((byte)(i >> 16));
            stream.WriteByte((byte)(i >> 8));
            stream.WriteByte((byte)i);
        }

        public static void WriteOpaque8(byte[] data, Stream stream)
        {
            WriteUint8((short)data.Length, stream);
            stream.Write(data, 0, data.Length);
        }

        public static void WriteOpaque16(byte[] data, Stream stream)
        {
            WriteUint16(data.Length, stream);
            stream.Write(data, 0, data.Length);
        }

        public static void WriteOpaque24(byte[] data, Stream stream)
        {
            WriteUint24(data.Length, stream);
            stream.Write(data, 0, data.Length);
        }

        public static byte[] ReadOpaque8(Stream stream)
        {
            short len = ReadUint8(stream);
            byte[] data = new byte[len];
            ReadFully(data, stream);
            return data;
        }

        public static byte[] ReadOpaque16(Stream stream)
        {
            int len = ReadUint16(stream);
            byte[] data = new byte[len];
            ReadFully(data, stream);
            return data;
        }

        public static void ReadFully(byte[] data, Stream stream)
        {
            int read = 0;
            while (read < data.Length)
            {
                int r = stream.Read(data, read, data.Length - read);
                if (r == 0)
                    throw new EndOfStreamException();
                read += r;
            }
        }

        public static void CheckVersion(Stream stream, TlsProtocolHandler handler)
        {
            // TODO: Implement this
        }

        public static byte[] PRF(byte[] secret, string label, byte[] seed, int size)
        {
            // TODO: Implement this
            return new byte[size];
        }

        public static byte[] Concat(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Array.Copy(a, 0, c, 0, a.Length);
            Array.Copy(b, 0, c, a.Length, b.Length);
            return c;
        }

        public static void WriteGMTUnixTime(byte[] buffer, int offset)
        {
            // TODO: Implement this
        }

        public static void WriteVersion(Stream stream)
        {
            // TODO: Implement this
        }
    }
}
