using JRadius.Core.Client;
using JRadius.Core.Packet.Attribute;
using JRadius.Core.Util;

namespace JRadius.Core.Packet
{
    public class AccessRequest : RadiusRequest
    {
        public const byte CODE = 1;

        public AccessRequest()
        {
            _code = CODE;
        }

        public AccessRequest(RadiusClient client)
            : base(client)
        {
            _code = CODE;
        }

        public AccessRequest(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }

        public AccessRequest(RadiusClient client, AttributeList attributes)
            : base(client, attributes)
        {
            _code = CODE;
        }

        public override byte[] CreateAuthenticator(byte[] attributes, int offset, int attributesLength, string sharedSecret)
        {
            _authenticator = RadiusUtils.MakeRFC2865RequestAuthenticator(sharedSecret);
            return _authenticator;
        }
    }
}
