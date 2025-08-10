using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPInform : DHCPPacket
    {
        public const int CODE = 1024 + 8;

        public DHCPInform()
        {
            _code = CODE;
        }

        public DHCPInform(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
