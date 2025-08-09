using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static Dictionary<string, object> _packetHandlers = new Dictionary<string, object>(); // TODO: Replace object with PacketHandlerConfigurationItem
        private static Dictionary<string, object> _eventHandlers = new Dictionary<string, object>(); // TODO: Replace object with HandlerConfigurationItem
        private static Dictionary<string, DictionaryConfigurationItem> _dictionaries = new Dictionary<string, DictionaryConfigurationItem>();
        // private static JRConfigParser parser = new JRConfigParser(); // TODO: Implement JRConfigParser
        // private static CatalogFactory factory = CatalogFactory.getInstance(); // TODO: Find replacement for commons-chain
        private static object _logConfig; // TODO: Replace object with LogConfigurationItem

        public static void Initialize(Stream input, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(Configuration));
            _xmlCfg = XDocument.Load(input);
            _root = _xmlCfg.Root;

            _logger.LogInformation("Configuring JRadius Server....");

            // SetLogConfig();
            // SetGeneralOptions();
            // SetRealmManagerConfig();
            // SetSessionManagerConfig();
            // SetDictionaryConfigs();
            // SetPacketHandlersConfigs();
            // SetEventHandlersConfigs();
            // SetListenerConfigs();
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

        public static ICollection<object> GetPacketHandlers()
        {
            return _packetHandlers.Values;
        }

        public static ICollection<object> GetEventHandlers()
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
