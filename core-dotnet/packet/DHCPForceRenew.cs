using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPForceRenew : DHCPPacket
    {
        public const int CODE = 1024 + 9;

        public DHCPForceRenew()
        {
            _code = CODE;
        }

        public DHCPForceRenew(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
