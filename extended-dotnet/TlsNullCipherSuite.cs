using System;
using System.IO;

namespace JRadius.Extended.Tls
{
    public class TlsNullCipherSuite : TlsCipherSuite
    {
        protected override void Init(byte[] ms, byte[] cr, byte[] sr)
        {
            throw new TlsRuntimeException("Sorry, init of TLS_NULL_WITH_NULL_NULL is forbidden");
        }

        protected override byte[] EncodePlaintext(short type, byte[] plaintext, int offset, int len)
        {
            byte[] result = new byte[len];
            Array.Copy(plaintext, offset, result, 0, len);
            return result;
        }

        protected override byte[] DecodeCiphertext(short type, byte[] plaintext, int offset, int len, TlsProtocolHandler handler)
        {
            byte[] result = new byte[len];
            Array.Copy(plaintext, offset, result, 0, len);
            return result;
        }

        protected override short GetKeyExchangeAlgorithm()
        {
            return 0;
        }
    }
}
