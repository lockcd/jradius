using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.IO;

namespace JRadius.Extended.Tls
{
    public class TlsSRPKeyExchange : TlsKeyExchange
    {
        private readonly TlsProtocolHandler _handler;
        private readonly CertificateVerifyer _verifyer;
        private readonly short _keyExchange;
        private readonly ITlsSigner _tlsSigner;

        private AsymmetricKeyParameter _serverPublicKey = null;

        // TODO Need a way of providing these
        private byte[] _srpIdentity = null;
        private byte[] _srpPassword = null;

        private byte[] _s = null;
        private BigInteger _b = null;
        private readonly Srp6Client _srpClient = new Srp6Client();

        public TlsSRPKeyExchange(TlsProtocolHandler handler, CertificateVerifyer verifyer, short keyExchange)
        {
            switch (keyExchange)
            {
                case TlsKeyExchange.KE_SRP:
                    _tlsSigner = null;
                    break;
                case TlsKeyExchange.KE_SRP_RSA:
                    _tlsSigner = new TlsRSASigner();
                    break;
                case TlsKeyExchange.KE_SRP_DSS:
                    _tlsSigner = new TlsDSSSigner();
                    break;
                default:
                    throw new ArgumentException("unsupported key exchange algorithm");
            }

            _handler = handler;
            _verifyer = verifyer;
            _keyExchange = keyExchange;
        }

        public void SkipServerCertificate()
        {
            if (_tlsSigner != null)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
            }
        }

        public void ProcessServerCertificate(Certificate serverCertificate)
        {
            if (_tlsSigner == null)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
            }

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

            switch (_keyExchange)
            {
                case TlsKeyExchange.KE_SRP_RSA:
                    if (!(_serverPublicKey is RsaKeyParameters))
                    {
                        _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
                    }
                    ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
                    break;
                case TlsKeyExchange.KE_SRP_DSS:
                    if (!(_serverPublicKey is DsaPublicKeyParameters))
                    {
                        _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
                    }
                    break;
                default:
                    _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unsupported_certificate);
                    break;
            }

            if (!_verifyer.IsValid(serverCertificate.GetCerts()))
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_user_canceled);
            }
        }

        public void SkipServerKeyExchange()
        {
            _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
        }

        public void ProcessServerKeyExchange(Stream is_Renamed, SecurityParameters securityParameters)
        {
            Stream sigIn = is_Renamed;
            ISigner signer = null;

            if (_tlsSigner != null)
            {
                signer = InitSigner(_tlsSigner, securityParameters);
                sigIn = new SignerStream(is_Renamed, signer, null);
            }

            byte[] nBytes = TlsUtils.ReadOpaque16(sigIn);
            byte[] gBytes = TlsUtils.ReadOpaque16(sigIn);
            byte[] sBytes = TlsUtils.ReadOpaque8(sigIn);
            byte[] bBytes = TlsUtils.ReadOpaque16(sigIn);

            if (signer != null)
            {
                byte[] sigByte = TlsUtils.ReadOpaque16(is_Renamed);
                if (!signer.VerifySignature(sigByte))
                {
                    _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_bad_certificate);
                }
            }

            var n = new BigInteger(1, nBytes);
            var g = new BigInteger(1, gBytes);
            _s = sBytes;

            try
            {
                _b = Srp6Utilities.ValidatePublicValue(n, new BigInteger(1, bBytes));
            }
            catch (CryptoException)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
            }

            _srpClient.Init(n, g, new Sha1Digest(), new SecureRandom());
        }

        public byte[] GenerateClientKeyExchange()
        {
            return _srpClient.GenerateClientCredentials(_s, _srpIdentity, _srpPassword).ToByteArrayUnsigned();
        }

        public byte[] GeneratePremasterSecret()
        {
            try
            {
                return _srpClient.CalculateSecret(_b).ToByteArrayUnsigned();
            }
            catch (CryptoException)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
                return null;
            }
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

        private ISigner InitSigner(ITlsSigner tlsSigner, SecurityParameters securityParameters)
        {
            var signer = tlsSigner.CreateVerifyer(_serverPublicKey);
            signer.BlockUpdate(securityParameters.ClientRandom, 0, securityParameters.ClientRandom.Length);
            signer.BlockUpdate(securityParameters.ServerRandom, 0, securityParameters.ServerRandom.Length);
            return signer;
        }
    }
}
