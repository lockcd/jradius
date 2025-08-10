using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class AccessChallenge : AccessResponse
    {
        public const byte CODE = 11;

        public AccessChallenge()
        {
            _code = CODE;
        }

        public AccessChallenge(int id, AttributeList attributes)
            : base(id, attributes)
        {
            _code = CODE;
        }
    }
}
