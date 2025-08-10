namespace JRadius.Core.Packet.Attribute
{
    public class Attr_UnknownVSAttribute : VSAttribute
    {
        public Attr_UnknownVSAttribute(long vendor, long type)
        {
            _vendorId = vendor;
            _vsaAttributeType = type;
        }

        public override void Setup()
        {
            // No setup needed
        }
    }
}
