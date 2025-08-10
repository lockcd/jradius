using Org.BouncyCastle.Crypto;

namespace JRadius.Extended.Tls
{
    public interface ITlsSigner
    {
        byte[] CalculateRawSignature(AsymmetricKeyParameter privateKey, byte[] md5andsha1);
        ISigner CreateVerifyer(AsymmetricKeyParameter publicKey);
    }
}
