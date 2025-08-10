using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPNack : DHCPPacket
    {
        public const int CODE = 1024 + 6;

        public DHCPNack()
        {
            _code = CODE;
        }

        public DHCPNack(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
