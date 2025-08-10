using JRadius.Core.Packet.Attribute;
using JRadius.Core.Packet.Attribute.Value;

namespace JRadius.Dictionary
{
    public class Attr_Message_Authenticator : RadiusAttribute
    {
        public const int TYPE = 80;
        public const string NAME = "Message-Authenticator";

        public Attr_Message_Authenticator()
        {
        }

        public Attr_Message_Authenticator(byte[] data)
        {
            _attributeValue = new OctetsValue(data);
        }

        public override void Setup()
        {
            _attributeType = TYPE;
            _attributeName = NAME;
            _attributeValue = new OctetsValue();
        }
    }
}
