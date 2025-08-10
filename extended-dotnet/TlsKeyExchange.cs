using System.IO;

namespace JRadius.Extended.Tls
{
    public interface TlsKeyExchange
    {
        const short KE_RSA = 1;
        //    static final short KE_RSA_EXPORT = 2;
        const short KE_DHE_DSS = 3;
        //    static final short KE_DHE_DSS_EXPORT = 4;
        const short KE_DHE_RSA = 5;
        //    static final short KE_DHE_RSA_EXPORT = 6;
        const short KE_DH_DSS = 7;
        const short KE_DH_RSA = 8;
        //    static final short KE_DH_anon = 9;
        const short KE_SRP = 10;
        const short KE_SRP_DSS = 11;
        const short KE_SRP_RSA = 12;

        void SkipServerCertificate();
        void ProcessServerCertificate(Certificate serverCertificate);
        void SkipServerKeyExchange();
        void ProcessServerKeyExchange(Stream is_Renamed, SecurityParameters securityParameters);
        byte[] GenerateClientKeyExchange();
        byte[] GeneratePremasterSecret();
    }
}
