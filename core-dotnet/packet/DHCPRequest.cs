using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPRequest : DHCPPacket
    {
        public const int CODE = 1024 + 3;

        public DHCPRequest()
        {
            _code = CODE;
        }

        public DHCPRequest(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
