using System.Text;

namespace JRadius.Core.Packet.Attribute.Value
{
    public class EncryptedStringValue : OctetsValue
    {
        public EncryptedStringValue() { }

        public EncryptedStringValue(string s)
            : base(s != null ? Encoding.UTF8.GetBytes(s) : null)
        {
        }

        public EncryptedStringValue(byte[] b)
            : base(b)
        {
        }

        public override string ToString()
        {
            return "[Encrypted String]";
        }
    }
}
