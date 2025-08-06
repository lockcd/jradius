using System;
using System.IO;

namespace JRadius.Extended.Tls
{
    public class ByteQueue
    {
        private MemoryStream buffer = new MemoryStream();

        public void AddData(byte[] data, int offset, int len)
        {
            buffer.Write(data, offset, len);
        }

        public int Read(byte[] buf, int offset, int len, int skip)
        {
            long originalPosition = buffer.Position;
            buffer.Position = skip;
            int bytesRead = buffer.Read(buf, offset, len);
            buffer.Position = originalPosition;
            return bytesRead;
        }

        public void RemoveData(int len)
        {
            byte[] data = buffer.ToArray();
            buffer.SetLength(0);
            buffer.Write(data, len, data.Length - len);
        }

        public int Size
        {
            get { return (int)buffer.Length; }
        }
    }
}
