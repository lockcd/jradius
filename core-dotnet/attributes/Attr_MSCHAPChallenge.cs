using JRadius.Core.Packet.Attribute;
using JRadius.Core.Packet.Attribute.Value;

namespace JRadius.Dictionary
{
    public class Attr_MSCHAPChallenge : RadiusAttribute
    {
        public const int TYPE = 11;
        public const string NAME = "MS-CHAP-Challenge";

        public Attr_MSCHAPChallenge()
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
