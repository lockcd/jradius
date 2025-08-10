using JRadius.Core.Packet.Attribute;
using JRadius.Core.Packet.Attribute.Value;

namespace JRadius.Dictionary
{
    public class Attr_MSCHAP2Response : RadiusAttribute
    {
        public const int TYPE = 13;
        public const string NAME = "MS-CHAP2-Response";

        public Attr_MSCHAP2Response()
        {
        }

        public override void Setup()
        {
            _attributeType = TYPE;
            _attributeName = NAME;
            _attributeValue = new OctetsValue();
        }
    }
}
