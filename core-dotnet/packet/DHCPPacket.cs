using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public abstract class DHCPPacket : RadiusPacket
    {
        public DHCPPacket()
            : base()
        {
        }

        public DHCPPacket(AttributeList attributes)
            : base(attributes)
        {
        }
    }
}
