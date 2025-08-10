using JRadius.Core.Packet.Attribute;
using JRadius.Core.Packet.Attribute.Value;

namespace JRadius.Dictionary
{
    public class Attr_UserPassword : RadiusAttribute
    {
        public const int TYPE = 2;
        public const string NAME = "User-Password";

        public Attr_UserPassword()
        {
        }

        public override void Setup()
        {
            _attributeType = TYPE;
            _attributeName = NAME;
            _attributeValue = new EncryptedStringValue();
        }
    }
}
