using JRadius.Core.Packet.Attribute;
using JRadius.Core.Packet.Attribute.Value;

namespace JRadius.Dictionary
{
    public class Attr_EAPMessage : RadiusAttribute
    {
        public const int TYPE = 79;
        public const string NAME = "EAP-Message";

        public Attr_EAPMessage()
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
