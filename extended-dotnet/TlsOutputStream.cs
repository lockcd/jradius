using System.IO;

namespace JRadius.Extended.Tls
{
    public class TlsOutputStream : Stream
    {
        private TlsProtocolHandler handler;

        public TlsOutputStream(TlsProtocolHandler handler)
        {
            this.handler = handler;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new System.NotSupportedException();
        public override long Position { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }

        public override void Flush()
        {
            handler.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            handler.WriteData(buffer, offset, count);
        }
    }
}
