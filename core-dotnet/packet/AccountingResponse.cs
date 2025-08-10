using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public class AccountingResponse : RadiusResponse
    {
        public const byte CODE = 5;

        public AccountingResponse()
        {
            _code = CODE;
        }

        public AccountingResponse(int id, AttributeList attributes)
            : base(id, attributes)
        {
            _code = CODE;
        }
    }
}
