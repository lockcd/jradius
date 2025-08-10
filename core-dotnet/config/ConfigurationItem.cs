using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace JRadius.Core.Config
{
    public abstract class ConfigurationItem
    {
        protected XElement _root;
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClassName { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public ConfigurationItem(string name)
        {
            Name = name;
        }

        public ConfigurationItem(string name, string className)
        {
            Name = name;
            ClassName = className;
        }

        public ConfigurationItem(XElement node)
        {
            _root = node;
            Name = GetConfigString("name");
            Description = GetConfigString("description");
            ClassName = GetConfigString("class");
            SetProperties();
        }

        protected void SetProperties()
        {
            Properties = GetPropertiesFromConfig(_root);
        }

        public static Dictionary<string, string> GetPropertiesFromConfig(XElement root)
        {
            var map = new Dictionary<string, string>();
            var properties = root.Elements("property");
            foreach (var property in properties)
            {
                var name = property.Element("name")?.Value;
                var value = property.Element("value")?.Value;
                if (name != null)
                {
                    map[name] = value;
                }
            }
            return map;
        }

        public XElement GetRoot()
        {
            return _root;
        }

        protected string GetConfigString(string elementName)
        {
            return _root.Element(elementName)?.Value;
        }

        protected int GetConfigInt(string elementName)
        {
            var value = GetConfigString(elementName);
            int.TryParse(value, out int result);
            return result;
        }

        protected bool GetConfigBoolean(string elementName)
        {
            var value = GetConfigString(elementName);
            bool.TryParse(value, out bool result);
            return result;
        }

        public virtual string XmlKey()
        {
            return "no such key";
        }

        public override string ToString()
        {
            return $"{Name} [{ClassName}]: {Description} -- {string.Join(", ", Properties.Select(p => $"{p.Key}={p.Value}"))}";
        }
    }
}
