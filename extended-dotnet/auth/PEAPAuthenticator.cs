using JRadius.Core.Client;
using JRadius.Core.Client.Auth;
using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;

namespace JRadius.Extended.Auth
{
    public class PEAPAuthenticator : EAPTLSAuthenticator
    {
        public new const string NAME = "peap";
        private EAPAuthenticator _tunnelAuth;
        private RadiusPacket _tunnelRequest;

        public PEAPAuthenticator()
        {
            SetEAPType(EAP_PEAP);
        }

        public override void Init()
        {
            base.Init();
            _tunnelAuth = new EAPMSCHAPv2Authenticator(true);
        }

        public override string GetAuthName()
        {
            return NAME;
        }

        protected new bool IsCertificateRequired()
        {
            return false;
        }

        public override void SetupRequest(RadiusClient client, RadiusPacket p)
        {
            base.SetupRequest(client, p);
            _tunnelRequest = new AccessRequest();
            var attrs = _tunnelRequest.GetAttributes();
            // TODO: Implement attribute copying
            _tunnelAuth.SetupRequest(client, _tunnelRequest);
            _tunnelAuth.ProcessRequest(_tunnelRequest);
        }

        protected bool DoTunnelAuthentication(byte id, byte[] input)
        {
            byte[] output;
            if (input != null && input.Length > 0)
            {
                output = _tunnelAuth.DoEAP(input);
            }
            else
            {
                output = _tunnelAuth.EapResponse(EAP_IDENTITY, 0, GetUsername());
            }
            // TODO: PutAppBuffer(output);
            return true;
        }
    }
}
