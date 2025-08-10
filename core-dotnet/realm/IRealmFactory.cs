using System.Collections.Generic;
using System.Xml.Linq;

namespace JRadius.Core.Realm
{
    public interface IRealmFactory
    {
        JRadiusRealm GetRealm(string realmName);
        ICollection<JRadiusRealm> GetRealms();
        void SetConfig(XElement root);
    }
}
