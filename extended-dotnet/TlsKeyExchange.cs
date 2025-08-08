using System.IO;

namespace JRadius.Extended.Tls
{
    public interface TlsKeyExchange
    {
        void ProcessServerCertificate(Certificate serverCertificate);
        void SkipServerKeyExchange();
        byte[] GenerateClientKeyExchange();
        byte[] GeneratePremasterSecret();
        void SkipServerCertificate();
        void ProcessServerKeyExchange(Stream stream, SecurityParameters securityParameters);
    }
}
