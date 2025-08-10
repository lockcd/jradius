using JRadius.Core.Client;
using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class CoARequest : AccountingRequest
    {
        public const byte CODE = 43;

        public CoARequest()
        {
            _code = CODE;
        }

        public CoARequest(RadiusClient client, AttributeList attributes)
            : base(client, attributes)
        {
            _code = CODE;
        }
    }
}
