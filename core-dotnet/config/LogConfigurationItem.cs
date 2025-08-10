using System.Xml.Linq;

namespace JRadius.Core.Config
{
    public class LogConfigurationItem : ConfigurationItem
    {
        public static string XML_KEY = "radius-logger";

        public LogConfigurationItem(XElement node)
            : base(node)
        {
        }

        public override string XmlKey()
        {
            return XML_KEY;
        }
    }
}
