using JRadius.Core.Handler.Chain;
using JRadius.Core.Session;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace JRadius.Core.Config
{
    public static class Configuration
    {
        private static ILogger _logger;
        private static XDocument _xmlCfg;
        private static XElement _root;

        private static bool _debug;
        private static int _timeoutSeconds;
        private static FileInfo _configFile;
        private static Dictionary<string, ListenerConfigurationItem> _listeners = new Dictionary<string, ListenerConfigurationItem>();
        private static Dictionary<string, PacketHandlerConfigurationItem> _packetHandlers = new Dictionary<string, PacketHandlerConfigurationItem>();
        private static Dictionary<string, HandlerConfigurationItem> _eventHandlers = new Dictionary<string, HandlerConfigurationItem>();
        private static Dictionary<string, DictionaryConfigurationItem> _dictionaries = new Dictionary<string, DictionaryConfigurationItem>();
        private static JRConfigParser _parser = new JRConfigParser();
        private static CatalogFactory _factory = CatalogFactory.GetInstance();
        private static LogConfigurationItem _logConfig;

        private const string SESSION_MANAGER_KEY = "session-manager";
        private const string REALM_MANAGER_KEY = "realm-manager";
        private const string REQUESTER_KEY = "requester";
        private const string KEY_PROVIDER_KEY = "key-provider";
        private const string SESSION_FACTORY_KEY = "session-factory";
        private const string REALM_FACTORY_KEY = "realm-factory";


        public static void Initialize(Stream input, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(Configuration));
            _xmlCfg = XDocument.Load(input);
            _root = _xmlCfg.Root;

            _logger.LogInformation("Configuring JRadius Server....");

            SetLogConfig();
            SetGeneralOptions();
            SetRealmManagerConfig();
            SetSessionManagerConfig();
            SetDictionaryConfigs();
            SetPacketHandlersConfigs();
            SetEventHandlersConfigs();
            SetListenerConfigs();
        }

        private static void SetGeneralOptions()
        {
            bool.TryParse(_root.Element("debug")?.Value, out _debug);
            int.TryParse(_root.Element("timeout")?.Value, out _timeoutSeconds);

            var children = _root.Elements("chain-catalog");
            foreach (var node in children)
            {
                var catalogURL = node.Element("name")?.Value;
                if (catalogURL != null)
                {
                    _logger.LogDebug($"Loading Chains URL: {catalogURL}");
                    try
                    {
                        using (var stream = new FileStream(catalogURL, FileMode.Open, FileAccess.Read))
                        {
                            var catalog = _parser.Parse(stream);
                            _factory.AddCatalog(catalogURL, catalog);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error loading catalog chain: {catalogURL}");
                    }
                }
            }
        }

        private static void SetLogConfig()
        {
            var children = _root.Elements(LogConfigurationItem.XML_KEY);
            foreach (var node in children)
            {
                if (_logConfig != null)
                {
                    _logger.LogWarning("A RadiusLogger is already configured, skipping configuration");
                    return;
                }
                _logConfig = new LogConfigurationItem(node);
                // TODO: Setup the new logger
            }
        }

        private static void SetDictionaryConfigs()
        {
            var children = _root.Elements(DictionaryConfigurationItem.XML_KEY);
            foreach (var node in children)
            {
                var item = new DictionaryConfigurationItem(node);
                _dictionaries[item.Name] = item;
            }
        }

        private static void SetListenerConfigs()
        {
            var list = _root.Elements(ListenerConfigurationItem.XML_LIST_KEY);
            foreach (var l in list)
            {
                var children = l.Elements(ListenerConfigurationItem.XML_KEY);
                foreach (var node in children)
                {
                    var item = new ListenerConfigurationItem(node);
                    _listeners[item.Name] = item;
                }
            }
        }

        private static void SetPacketHandlersConfigs()
        {
            var list = _root.Elements(PacketHandlerConfigurationItem.XML_LIST_KEY);
            foreach (var l in list)
            {
                var children = l.Elements(PacketHandlerConfigurationItem.XML_KEY);
                foreach (var node in children)
                {
                    var item = new PacketHandlerConfigurationItem(node);
                    _packetHandlers[item.Name] = item;
                }
            }
        }

        private static void SetEventHandlersConfigs()
        {
            var list = _root.Elements(HandlerConfigurationItem.XML_LIST_KEY);
            foreach (var l in list)
            {
                var children = l.Elements(HandlerConfigurationItem.XML_KEY);
                foreach (var node in children)
                {
                    var item = new HandlerConfigurationItem(node);
                    _eventHandlers[item.Name] = item;
                }
            }
        }

        private static void SetSessionManagerConfig()
        {
            _logger.LogInformation("Initializing session manager");
            var list = _root.Elements(SESSION_MANAGER_KEY);
            foreach (var node in list)
            {
                var clazz = node.Element("class")?.Value;
                var requester = node.Element(REQUESTER_KEY)?.Value;
                var keyProvider = node.Element(KEY_PROVIDER_KEY)?.Value;
                var sessionFactory = node.Element(SESSION_FACTORY_KEY)?.Value;

                if (clazz != null)
                {
                    try
                    {
                        _logger.LogDebug($"Session Manager ({requester}): {clazz}");
                        var manager = (JRadiusSessionManager)GetBean(clazz);
                        JRadiusSessionManager.SetManager(requester, manager);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);
                    }
                }

                if (keyProvider != null)
                {
                    try
                    {
                        _logger.LogDebug($"Session Key Provider ({requester}): {keyProvider}");
                        var provider = (ISessionKeyProvider)GetBean(keyProvider);
                        JRadiusSessionManager.GetManager(requester).SetSessionKeyProvider(requester, provider);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);
                    }
                }

                if (sessionFactory != null)
                {
                    try
                    {
                        _logger.LogDebug($"Session Factory ({requester}): {sessionFactory}");
                        var factory = (ISessionFactory)GetBean(sessionFactory);
                        // factory.SetConfig(node); // TODO: ISessionFactory needs a SetConfig method
                        JRadiusSessionManager.GetManager(requester).SetSessionFactory(requester, factory);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);
                    }
                }
            }
        }

        private static void SetRealmManagerConfig()
        {
            _logger.LogInformation("Initializing realm manager");
            var list = _root.Elements(REALM_MANAGER_KEY);
            foreach (var node in list)
            {
                // TODO: Implement this method. Requires GetBean and realm classes.
            }
        }

        public static object GetBean(string name)
        {
            if (name == null) return null;
            Type type = Type.GetType(name);
            if (type == null) return null;
            return Activator.CreateInstance(type);
        }

        public static bool IsDebug()
        {
            return _debug;
        }

        public static string GetConfigFileDir()
        {
            if (_configFile == null) return ".";
            return _configFile.DirectoryName;
        }

        public static ICollection<PacketHandlerConfigurationItem> GetPacketHandlers()
        {
            return _packetHandlers.Values;
        }

        public static ICollection<HandlerConfigurationItem> GetEventHandlers()
        {
            return _eventHandlers.Values;
        }

        public static ICollection<DictionaryConfigurationItem> GetDictionaryConfigs()
        {
            return _dictionaries.Values;
        }

        public static ICollection<ListenerConfigurationItem> GetListenerConfigs()
        {
            return _listeners.Values;
        }

        public static int GetTimeoutSeconds()
        {
            return _timeoutSeconds;
        }
    }
}
