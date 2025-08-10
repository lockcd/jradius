using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPRelease : DHCPPacket
    {
        public const int CODE = 1024 + 7;

        public DHCPRelease()
        {
            _code = CODE;
        }

        public DHCPRelease(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
