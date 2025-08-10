using System;
using System.Collections.Generic;

namespace JRadius.Core.Realm
{
    public class JRadiusRealmManager
    {
        private static JRadiusRealmManager _defaultManager = new JRadiusRealmManager();

        private Dictionary<string, IRealmFactory> _factories = new Dictionary<string, IRealmFactory>();

        public static JRadiusRealmManager GetManager()
        {
            return _defaultManager;
        }

        public void SetRealmFactory(string name, IRealmFactory factory)
        {
            _factories[name] = factory;
        }

        public IRealmFactory GetRealmFactory(object name)
        {
            var key = name?.ToString();
            _factories.TryGetValue(key, out IRealmFactory factory);
            if (factory == null && key != null)
            {
                _factories.TryGetValue(null, out factory);
            }
            return factory;
        }

        public JRadiusRealm GetRealm(string realm)
        {
            foreach (var factory in _factories.Values)
            {
                var r = factory.GetRealm(realm);
                if (r != null) return r;
            }
            return null;
        }

        public static JRadiusRealm Get(string requestor, string realm)
        {
            return _defaultManager.GetRealmFactory(requestor).GetRealm(realm);
        }

        public static JRadiusRealm Get(string realm)
        {
            return _defaultManager.GetRealm(realm);
        }
    }
}
