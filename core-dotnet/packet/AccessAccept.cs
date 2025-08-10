using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class AccessAccept : AccessResponse
    {
        public const byte CODE = 2;

        public AccessAccept()
        {
            _code = CODE;
        }

        public AccessAccept(int id, AttributeList attributes)
            : base(id, attributes)
        {
            _code = CODE;
        }
    }
}
