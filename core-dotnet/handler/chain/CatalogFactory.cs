using System.Collections.Generic;

namespace JRadius.Core.Handler.Chain
{
    public class CatalogFactory
    {
        private static CatalogFactory _instance = new CatalogFactory();
        private Dictionary<string, CatalogBase> _catalogs = new Dictionary<string, CatalogBase>();
        private CatalogBase _defaultCatalog;

        public static CatalogFactory GetInstance()
        {
            return _instance;
        }

        public CatalogBase GetCatalog(string name = null)
        {
            if (name == null)
            {
                return _defaultCatalog;
            }
            _catalogs.TryGetValue(name, out CatalogBase catalog);
            return catalog;
        }

        public void AddCatalog(string name, CatalogBase catalog)
        {
            if (_defaultCatalog == null)
            {
                _defaultCatalog = catalog;
            }
            _catalogs[name] = catalog;
        }
    }
}
