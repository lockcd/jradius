using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;

namespace JRadius.Extended.Tls
{
    public class TlsRSAKeyExchange : TlsKeyExchange
    {
        private readonly TlsProtocolHandler _handler;
        private readonly CertificateVerifyer _verifyer;

        private AsymmetricKeyParameter _serverPublicKey = null;
        private RsaKeyParameters _rsaServerPublicKey = null;
        private byte[] _premasterSecret;

        public TlsRSAKeyExchange(TlsProtocolHandler handler, CertificateVerifyer verifyer)
        {
            _handler = handler;
            _verifyer = verifyer;
        }

        public void SkipServerCertificate()
        {
            _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
        }

        public void ProcessServerCertificate(Certificate serverCertificate)
        {
            var x509Cert = serverCertificate.Certs[0];
            var keyInfo = x509Cert.SubjectPublicKeyInfo;

            try
            {
                _serverPublicKey = PublicKeyFactory.CreateKey(keyInfo);
            }
            catch (Exception)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unsupported_certificate);
            }

            if (_serverPublicKey.IsPrivate)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_internal_error);
            }

            if (!(_serverPublicKey is RsaKeyParameters))
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
            }
            ValidateKeyUsage(x509Cert, KeyUsage.KeyEncipherment);
            _rsaServerPublicKey = ValidateRSAPublicKey((RsaKeyParameters)_serverPublicKey);

            if (!_verifyer.IsValid(serverCertificate.GetCerts()))
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_user_canceled);
            }
        }

        public void SkipServerKeyExchange()
        {
            // OK
        }

        public void ProcessServerKeyExchange(Stream is_Renamed, SecurityParameters securityParameters)
        {
            _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
        }

        public byte[] GenerateClientKeyExchange()
        {
            _premasterSecret = new byte[48];
            _handler.Random.NextBytes(_premasterSecret);
            TlsUtils.WriteVersion(_premasterSecret, 0);

            var encoding = new Pkcs1Encoding(new RsaBlindedEngine());
            encoding.Init(true, new ParametersWithRandom(_rsaServerPublicKey, _handler.Random));

            try
            {
                return encoding.ProcessBlock(_premasterSecret, 0, _premasterSecret.Length);
            }
            catch (InvalidCipherTextException)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_internal_error);
                return null; // Unreachable!
            }
        }

        public byte[] GeneratePremasterSecret()
        {
            byte[] tmp = _premasterSecret;
            _premasterSecret = null;
            return tmp;
        }

        private void ValidateKeyUsage(X509CertificateStructure c, int keyUsageBits)
        {
            var exts = c.TbsCertificate.Extensions;
            if (exts != null)
            {
                var ext = exts.GetExtension(X509Extensions.KeyUsage);
                if (ext != null)
                {
                    var ku = KeyUsage.GetInstance(ext);
                    int bits = ku.GetBytes()[0] & 0xff;
                    if ((bits & keyUsageBits) != keyUsageBits)
                    {
                        _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
                    }
                }
            }
        }

        private RsaKeyParameters ValidateRSAPublicKey(RsaKeyParameters key)
        {
            if (!key.Exponent.IsProbablePrime(2))
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
            }
            return key;
        }
    }
}
