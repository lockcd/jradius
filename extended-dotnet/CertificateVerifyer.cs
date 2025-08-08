namespace JRadius.Extended.Tls
{
    public interface CertificateVerifyer
    {
        bool IsValid(Certificate cert);
    }
}
