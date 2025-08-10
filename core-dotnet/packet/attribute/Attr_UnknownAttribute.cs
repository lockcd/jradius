namespace JRadius.Core.Packet.Attribute
{
    public class Attr_UnknownAttribute : RadiusAttribute
    {
        public Attr_UnknownAttribute(long type)
        {
            _attributeType = type;
        }

        public override void Setup()
        {
            // No setup needed
        }
    }
}
