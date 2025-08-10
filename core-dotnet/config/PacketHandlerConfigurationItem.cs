using System.Xml.Linq;

namespace JRadius.Core.Config
{
    public class PacketHandlerConfigurationItem : HandlerConfigurationItem
    {
        public static readonly string XML_LIST_KEY = "packet-handlers";
        public static readonly new string XML_KEY = "packet-handler";
        public static readonly string XML_KEY_ALT = "request-handler";

        public PacketHandlerConfigurationItem(string name)
            : base(name)
        {
        }

        public PacketHandlerConfigurationItem(string name, string className)
            : base(name, className)
        {
        }

        public PacketHandlerConfigurationItem(XElement node)
            : base(node)
        {
        }

        public override string XmlKey()
        {
            return XML_KEY;
        }
    }
}
