using JRadius.Core.Packet.Attribute;
using JRadius.Core.Packet.Attribute.Value;

namespace JRadius.Dictionary
{
    public class Attr_CHAPChallenge : RadiusAttribute
    {
        public const int TYPE = 60;
        public const string NAME = "CHAP-Challenge";

        public Attr_CHAPChallenge()
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
