using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class AccessReject : AccessResponse
    {
        public const byte CODE = 3;

        public AccessReject()
        {
            _code = CODE;
        }

        public AccessReject(int id, AttributeList attributes)
            : base(id, attributes)
        {
            _code = CODE;
        }
    }
}
