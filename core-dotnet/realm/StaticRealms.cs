using System.Collections.Generic;
using System.Xml.Linq;

namespace JRadius.Core.Realm
{
    public class StaticRealms : IRealmFactory
    {
        protected Dictionary<string, JRadiusRealm> _realms = new Dictionary<string, JRadiusRealm>();

        public JRadiusRealm GetRealm(string realmName)
        {
            _realms.TryGetValue(realmName, out JRadiusRealm realm);
            return realm;
        }

        public ICollection<JRadiusRealm> GetRealms()
        {
            return _realms.Values;
        }

        public void SetConfig(XElement root)
        {
            var list = root.Elements("realm");
            foreach (var node in list)
            {
                var realm = new RadiusRealm
                {
                    Realm = node.Element("name")?.Value,
                    Server = node.Element("server")?.Value,
                    SharedSecret = node.Element("sharedSecret")?.Value,
                    AuthPort = int.Parse(node.Element("authPort")?.Value ?? "0"),
                    AcctPort = int.Parse(node.Element("acctPort")?.Value ?? "0")
                };
                _realms[realm.Realm] = realm;
            }
        }
    }
}
