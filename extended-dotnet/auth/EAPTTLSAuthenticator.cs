using JRadius.Core.Client;
using JRadius.Core.Client.Auth;
using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using System.IO;

namespace JRadius.Extended.Auth
{
    public class EAPTTLSAuthenticator : EAPTLSAuthenticator, ITunnelAuthenticator
    {
        public new const string NAME = "eap-ttls";
        private string _innerProtocol = "pap";
        private IRadiusAuthenticator _tunnelAuth;
        private RadiusPacket _tunnelRequest;
        private RadiusPacket _tunnelChallenge;
        private AttributeList _tunneledAttributes;
        private static readonly DiameterFormat _diameterFormat = new DiameterFormat();

        public EAPTTLSAuthenticator()
        {
            SetEAPType(EAP_TTLS);
        }

        public override void Init()
        {
            base.Init();
            _tunnelAuth = RadiusClient.GetAuthProtocol(GetInnerProtocol());
            if (_tunnelAuth == null || _tunnelAuth is MSCHAPv2Authenticator || _tunnelAuth is MSCHAPv1Authenticator || _tunnelAuth is CHAPAuthenticator)
            {
                throw new System.Exception("You can not currently use " + _tunnelAuth.GetAuthName() + " within a TLS Tunnel because of limitations in Java 1.5.");
            }
        }

        protected new bool IsCertificateRequired()
        {
            return false;
        }

        public override string GetAuthName()
        {
            return NAME;
        }

        public void SetTunneledAttributes(AttributeList attributes)
        {
            _tunneledAttributes = attributes;
        }

        public override void SetupRequest(RadiusClient client, RadiusPacket p)
        {
            base.SetupRequest(client, p);
            _tunnelRequest = new AccessRequest(_tunneledAttributes);
            var attrs = _tunnelRequest.GetAttributes();
            // TODO: Implement attribute copying
            _tunnelAuth.SetupRequest(client, _tunnelRequest);
            if (!(_tunnelAuth is PAPAuthenticator))
            {
                _tunnelAuth.ProcessRequest(_tunnelRequest);
            }
        }

        protected bool DoTunnelAuthentication(byte id, byte[] input)
        {
            if (_tunnelChallenge != null && input != null)
            {
                var list = _tunnelChallenge.GetAttributes();
                list.Clear();
                var buffer = new MemoryStream(input);
                _diameterFormat.UnpackAttributes(list, buffer, (int)buffer.Length, false);
                if (_tunnelAuth is EAPAuthenticator && _tunnelChallenge.FindAttribute(79) == null)
                {
                    _tunnelAuth.SetupRequest(_client, _tunnelRequest);
                }
                else
                {
                    _tunnelAuth.ProcessChallenge(_tunnelRequest, _tunnelChallenge);
                }
            }
            else
            {
                _tunnelChallenge = new AccessChallenge();
            }

            var outBuffer = new MemoryStream(1500);
            _diameterFormat.PackAttributeList(_tunnelRequest.GetAttributes(), outBuffer, true);
            // TODO: PutAppBuffer(outBuffer.ToArray(), 0, (int)outBuffer.Position);
            return true;
        }

        public string GetInnerProtocol()
        {
            return _innerProtocol;
        }

        public void SetInnerProtocol(string innerProtocol)
        {
            _innerProtocol = innerProtocol;
        }
    }
}
