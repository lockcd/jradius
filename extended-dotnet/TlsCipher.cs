using System.IO;

namespace JRadius.Extended.Tls
{
    public interface TlsCipher
    {
        byte[] EncodePlaintext(short type, byte[] plaintext, int offset, int len);
        byte[] DecodeCiphertext(short type, byte[] ciphertext, int offset, int len);
    }
}
