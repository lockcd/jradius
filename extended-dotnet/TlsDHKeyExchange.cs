using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.IO;

namespace JRadius.Extended.Tls
{
    public class TlsDHKeyExchange : TlsKeyExchange
    {
        private static readonly BigInteger ONE = BigInteger.One;
        private static readonly BigInteger TWO = BigInteger.Two;

        private readonly TlsProtocolHandler _handler;
        private readonly ICertificateVerifyer _verifyer;
        private readonly short _keyExchange;
        private readonly ITlsSigner _tlsSigner;

        private AsymmetricKeyParameter _serverPublicKey = null;
        private DHPublicKeyParameters _dhAgreeServerPublicKey = null;
        private AsymmetricCipherKeyPair _dhAgreeClientKeyPair = null;

        public TlsDHKeyExchange(TlsProtocolHandler handler, CertificateVerifyer verifyer, short keyExchange)
        {
            switch (keyExchange)
            {
                case TlsKeyExchange.KE_DH_RSA:
                case TlsKeyExchange.KE_DH_DSS:
                    _tlsSigner = null;
                    break;
                case TlsKeyExchange.KE_DHE_RSA:
                    _tlsSigner = new TlsRSASigner();
                    break;
                case TlsKeyExchange.KE_DHE_DSS:
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

            switch (_keyExchange)
            {
                case TlsKeyExchange.KE_DH_DSS:
                    if (!(_serverPublicKey is DHPublicKeyParameters))
                    {
                        _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
                    }
                    _dhAgreeServerPublicKey = ValidateDHPublicKey((DHPublicKeyParameters)_serverPublicKey);
                    break;
                case TlsKeyExchange.KE_DH_RSA:
                    if (!(_serverPublicKey is DHPublicKeyParameters))
                    {
                        _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
                    }
                    _dhAgreeServerPublicKey = ValidateDHPublicKey((DHPublicKeyParameters)_serverPublicKey);
                    break;
                case TlsKeyExchange.KE_DHE_RSA:
                    if (!(_serverPublicKey is RsaKeyParameters))
                    {
                        _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_certificate_unknown);
                    }
                    ValidateKeyUsage(x509Cert, KeyUsage.DigitalSignature);
                    break;
                case TlsKeyExchange.KE_DHE_DSS:
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
            if (_tlsSigner != null)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
            }
        }

        public void ProcessServerKeyExchange(Stream is_Renamed, SecurityParameters securityParameters)
        {
            if (_tlsSigner == null)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_unexpected_message);
            }

            Stream sigIn = is_Renamed;
            ISigner signer = null;

            if (_tlsSigner != null)
            {
                signer = InitSigner(_tlsSigner, securityParameters);
                sigIn = new SignerStream(is_Renamed, signer, null);
            }

            byte[] pBytes = TlsUtils.ReadOpaque16(sigIn);
            byte[] gBytes = TlsUtils.ReadOpaque16(sigIn);
            byte[] YsBytes = TlsUtils.ReadOpaque16(sigIn);

            if (signer != null)
            {
                byte[] sigByte = TlsUtils.ReadOpaque16(is_Renamed);
                if (!signer.VerifySignature(sigByte))
                {
                    _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_bad_certificate);
                }
            }

            var p = new BigInteger(1, pBytes);
            var g = new BigInteger(1, gBytes);
            var Ys = new BigInteger(1, YsBytes);

            _dhAgreeServerPublicKey = ValidateDHPublicKey(new DHPublicKeyParameters(Ys, new DHParameters(p, g)));
        }

        public byte[] GenerateClientKeyExchange()
        {
            var dhGen = new DHBasicKeyPairGenerator();
            dhGen.Init(new DHKeyGenerationParameters(new SecureRandom(), _dhAgreeServerPublicKey.Parameters));
            _dhAgreeClientKeyPair = dhGen.GenerateKeyPair();
            var Yc = ((DHPublicKeyParameters)_dhAgreeClientKeyPair.Public).Y;
            return Yc.ToByteArrayUnsigned();
        }

        public byte[] GeneratePremasterSecret()
        {
            var dhAgree = new DHBasicAgreement();
            dhAgree.Init(_dhAgreeClientKeyPair.Private);
            var agreement = dhAgree.CalculateAgreement(_dhAgreeServerPublicKey);
            return agreement.ToByteArrayUnsigned();
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

        private DHPublicKeyParameters ValidateDHPublicKey(DHPublicKeyParameters key)
        {
            var Y = key.Y;
            var parameters = key.Parameters;
            var p = parameters.P;
            var g = parameters.G;

            if (!p.IsProbablePrime(2))
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
            }
            if (g.CompareTo(TWO) < 0 || g.CompareTo(p.Subtract(TWO)) > 0)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
            }
            if (Y.CompareTo(TWO) < 0 || Y.CompareTo(p.Subtract(ONE)) > 0)
            {
                _handler.FailWithError(TlsProtocolHandler.AL_fatal, TlsProtocolHandler.AP_illegal_parameter);
            }

            return key;
        }
    }
}
