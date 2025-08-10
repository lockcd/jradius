using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPDecline : DHCPPacket
    {
        public const int CODE = 1024 + 4;

        public DHCPDecline()
        {
            _code = CODE;
        }

        public DHCPDecline(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
