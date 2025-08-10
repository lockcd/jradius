using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class DHCPAck : DHCPPacket
    {
        public const int CODE = 1024 + 5;

        public DHCPAck()
        {
            _code = CODE;
        }

        public DHCPAck(AttributeList attributes)
            : base(attributes)
        {
            _code = CODE;
        }
    }
}
