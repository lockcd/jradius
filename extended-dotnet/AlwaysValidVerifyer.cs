using Org.BouncyCastle.Asn1.X509;

namespace JRadius.Extended.Tls
{
    /// <summary>
    /// A certificate verifyer, that will always return true.
    /// DO NOT USE THIS FILE UNLESS YOU KNOW EXACTLY WHAT YOU ARE DOING.
    /// </summary>
    public class AlwaysValidVerifyer : ICertificateVerifyer
    {
        /// <summary>
        /// Return true.
        /// </summary>
        public bool IsValid(X509CertificateStructure[] certs)
        {
            return true;
        }
    }
}
