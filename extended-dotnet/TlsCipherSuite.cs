using System.IO;

namespace JRadius.Extended.Tls
{
    public abstract class TlsCipherSuite
    {
        protected const short KE_RSA = 1;
        protected const short KE_RSA_EXPORT = 2;
        protected const short KE_DHE_DSS = 3;
        protected const short KE_DHE_DSS_EXPORT = 4;
        protected const short KE_DHE_RSA = 5;
        protected const short KE_DHE_RSA_EXPORT = 6;
        protected const short KE_DH_DSS = 7;
        protected const short KE_DH_RSA = 8;
        protected const short KE_DH_anon = 9;
        protected const short KE_SRP = 10;
        protected const short KE_SRP_RSA = 11;
        protected const short KE_SRP_DSS = 12;

        protected abstract void Init(byte[] ms, byte[] cr, byte[] sr);

        protected abstract byte[] EncodePlaintext(short type, byte[] plaintext, int offset, int len);

        protected abstract byte[] DecodeCiphertext(short type, byte[] plaintext, int offset, int len, TlsProtocolHandler handler);

        protected abstract short GetKeyExchangeAlgorithm();
    }
}
