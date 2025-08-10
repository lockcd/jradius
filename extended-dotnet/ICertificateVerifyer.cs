using Org.BouncyCastle.Asn1.X509;

namespace JRadius.Extended.Tls
{
    public interface ICertificateVerifyer
    {
        bool IsValid(X509CertificateStructure[] certs);
    }
}
