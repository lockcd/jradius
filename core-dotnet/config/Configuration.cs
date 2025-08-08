using System.Collections.Generic;

namespace net.jradius.core.config
{
    public static class Configuration
    {
        public static List<DictionaryConfigurationItem> GetDictionaryConfigs()
        {
            // This will be loaded from a configuration file in a real implementation
            return new List<DictionaryConfigurationItem>();
        }

        public static List<ListenerConfigurationItem> GetListenerConfigs()
        {
            // This will be loaded from a configuration file in a real implementation
            return new List<ListenerConfigurationItem>();
        }
    }
}
