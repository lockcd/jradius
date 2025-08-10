using JRadius.Core.Client;
using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DisconnectRequest : AccountingRequest
    {
        public const byte CODE = 40;

        public DisconnectRequest()
        {
            _code = CODE;
        }

        public DisconnectRequest(RadiusClient client, AttributeList attributes)
            : base(client, attributes)
        {
            _code = CODE;
        }
    }
}
