using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public abstract class AccessResponse : RadiusResponse
    {
        public AccessResponse()
        {
        }

        public AccessResponse(int id, AttributeList attributes)
            : base(id, attributes)
        {
        }
    }
}
