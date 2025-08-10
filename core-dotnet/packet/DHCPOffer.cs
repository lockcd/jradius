using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPOffer : DHCPPacket
    {
        public const int CODE = 1024 + 2;

        public DHCPOffer()
        {
            _code = CODE;
        }

        public DHCPOffer(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
