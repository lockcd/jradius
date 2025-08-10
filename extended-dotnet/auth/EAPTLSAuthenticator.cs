using JRadius.Core.Client;
using JRadius.Core.Client.Auth;
using JRadius.Core.Packet;
using JRadius.Extended.Tls;
using JRadius.Extended.Util;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace JRadius.Extended.Auth
{
    public class EAPTLSAuthenticator : EAPAuthenticator
    {
        public const string NAME = "eap-tls";

        private string _keyFileType;
        private string _keyFile;
        private string _keyPassword;
        private string _caFileType;
        private string _caFile;
        private string _caPassword;
        private bool _trustAll = false;

        private TlsProtocolHandler _handler = new TlsProtocolHandler();
        private AlwaysValidVerifyer _verifyer = new AlwaysValidVerifyer();
        private DefaultTlsClient _tlsClient = null;
        private X509Certificate2Collection _keyManagers = null;
        private X509Certificate2Collection _trustManagers = null;

        public EAPTLSAuthenticator()
        {
            SetEAPType(EAP_TLS);
            _keyFileType = "pkcs12";
            _keyPassword = "";
            _caFileType = "pkcs12";
            _caPassword = "";
        }

        public override void SetupRequest(RadiusClient client, RadiusPacket p)
        {
            base.SetupRequest(client, p);
            Init();
        }

        public void Init()
        {
            try
            {
                if (!string.IsNullOrEmpty(_keyFile))
                {
                    _keyManagers = KeyStoreUtil.LoadKeyManager(_keyFileType, new FileStream(_keyFile, FileMode.Open), _keyPassword);
                }

                if (_trustAll)
                {
                    // TODO: Set up trust all manager
                }
                else if (!string.IsNullOrEmpty(_caFile))
                {
                    _trustManagers = KeyStoreUtil.LoadTrustManager(_caFileType, new FileStream(_caFile, FileMode.Open), _caPassword);
                }

                // TODO: Implement the rest of the init logic
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override string GetAuthName()
        {
            return NAME;
        }

        public override byte[] DoEAPType(byte id, byte[] data)
        {
            // TODO: Implement EAP-TLS logic
            return null;
        }
    }
}
