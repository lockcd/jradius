using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace JRadius.Core.Config
{
    public class HandlerConfigurationItem : ConfigurationItem
    {
        public static readonly string XML_LIST_KEY = "event-handlers";
        public static readonly string XML_KEY = "event-handler";

        public static readonly string TYPE_KEY = "type";
        public static readonly string SENDER_KEY = "sender";
        public static readonly string HANDLER_KEY = "handler";
        public static readonly string CATALOG_KEY = "catalog";

        private List<string> _possibleTypes;
        public List<string> HandleTypes { get; set; }
        public List<string> Senders { get; set; }

        public string HandlerName { get; set; }
        public string CatalogName { get; set; }

        public HandlerConfigurationItem(string name)
            : base(name)
        {
        }

        public HandlerConfigurationItem(string name, string className)
            : base(name, className)
        {
        }

        public HandlerConfigurationItem(XElement node)
            : base(node)
        {
            _possibleTypes = node.Elements("handle").Elements("type").Select(t => t.Value).ToList();
            var type = GetConfigString(TYPE_KEY);
            var sender = GetConfigString(SENDER_KEY);
            HandlerName = GetConfigString(HANDLER_KEY);
            CatalogName = GetConfigString(CATALOG_KEY);
            SetSenders(sender);
            SetHandleTypes(type);
        }

        public void SetHandleTypes(string handleTypes)
        {
            var list = new List<string>();
            if (handleTypes == null) handleTypes = "";
            var types = handleTypes.Split(',').Select(s => s.Trim()).ToArray();

            if (types != null)
            {
                foreach (var t in types)
                {
                    if (t.Length > 0)
                    {
                        if (_possibleTypes == null ||
                            !_possibleTypes.Any() ||
                            _possibleTypes.Contains(t))
                        {
                            list.Add(t);
                        }
                    }
                }
            }
            HandleTypes = list;
        }

        public void SetSenders(string sender)
        {
            var list = new List<string>();
            if (sender == null) sender = "";
            var types = sender.Split(',').Select(s => s.Trim()).ToArray();

            if (types != null)
            {
                foreach (var t in types)
                {
                    if (t.Length > 0)
                    {
                        list.Add(t);
                    }
                }
            }
            Senders = list;
        }

        public List<string> GetPossibleTypes()
        {
            return _possibleTypes;
        }

        public void SetPossibleTypes(List<string> possibleTypes)
        {
            _possibleTypes = possibleTypes;
        }

        public bool HandlesType(string type)
        {
            if (!HandleTypes.Any()) return true;
            if (HandleTypes.Contains(type)) return true;
            return false;
        }

        public bool HandlesSender(object sender)
        {
            if (!Senders.Any()) return true;
            if (Senders.Contains(sender.ToString())) return true;
            return false;
        }
    }
}
