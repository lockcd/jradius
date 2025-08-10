using System;

namespace JRadius.Extended.Tls
{
    public class TlsNullCipher : TlsCipher
    {
        public byte[] EncodePlaintext(short type, byte[] plaintext, int offset, int len)
        {
            return CopyData(plaintext, offset, len);
        }

        public byte[] DecodeCiphertext(short type, byte[] ciphertext, int offset, int len)
        {
            return CopyData(ciphertext, offset, len);
        }

        private byte[] CopyData(byte[] text, int offset, int len)
        {
            byte[] result = new byte[len];
            Array.Copy(text, offset, result, 0, len);
            return result;
        }
    }
}
