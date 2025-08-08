using System;
using System.IO;

namespace JRadius.Extended.Tls
{
    public class RecordStream
    {
        private TlsProtocolHandler handler;
        private Stream inputStream;
        private Stream outputStream;

        public RecordStream(TlsProtocolHandler handler, Stream inputStream, Stream outputStream)
        {
            this.handler = handler;
            this.inputStream = inputStream;
            this.outputStream = outputStream;
        }

        public RecordStream(TlsProtocolHandler handler)
        {
            this.handler = handler;
        }

        public void SetInputStream(Stream stream)
        {
            this.inputStream = stream;
        }

        public void SetOutputStream(Stream stream)
        {
            this.outputStream = stream;
        }

        public bool HasMore()
        {
            return inputStream.Position < inputStream.Length;
        }

        public void UpdateHandshakeData(byte[] buf, int offset, int len)
        {
            // TODO: Implement this
        }

        public void WriteMessage(short type, byte[] message, int offset, int length)
        {
            // TODO: Implement this
        }

        public void ReadData()
        {
            // TODO: Implement this
        }

        public void Close()
        {
            inputStream.Close();
            outputStream.Close();
        }

        public void Flush()
        {
            outputStream.Flush();
        }

        public void serverClientSpecReceived()
        {
            // TODO: Implement this
        }

        public void clientCipherSpecDecided(TlsCipher tlsCipher)
        {
            // TODO: Implement this
        }

        public byte[] getCurrentHash()
        {
            // TODO: Implement this
            return new byte[0];
        }
    }
}
