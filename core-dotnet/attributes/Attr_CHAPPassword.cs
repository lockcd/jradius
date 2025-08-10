using JRadius.Core.Packet.Attribute;
using JRadius.Core.Packet.Attribute.Value;

namespace JRadius.Dictionary
{
    public class Attr_CHAPPassword : RadiusAttribute
    {
        public const int TYPE = 3;
        public const string NAME = "CHAP-Password";

        public Attr_CHAPPassword()
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
