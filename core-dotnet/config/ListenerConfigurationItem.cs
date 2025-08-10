using JRadius.Core.Handler;
using JRadius.Core.Handler.Chain;
using System.Collections.Generic;
using System.Xml.Linq;

namespace JRadius.Core.Config
{
    public class ListenerConfigurationItem : ConfigurationItem
    {
        public static string XML_LIST_KEY = "listeners";
        public static string XML_KEY = "listener";

        private List<IJRCommand> _requestHandlers;
        private List<IJRCommand> _eventHandlers;
        private string _processorClassName;
        private int _numberOfThreads;

        private static readonly string PROC_CLASS_KEY = "processor-class";
        private static readonly string PROC_THREADS_KEY = "processor-threads";

        public ListenerConfigurationItem(XElement node)
            : base(node)
        {
            _processorClassName = GetConfigString(PROC_CLASS_KEY);
            _numberOfThreads = GetConfigInt(PROC_THREADS_KEY);
            if (_numberOfThreads == 0)
            {
                _numberOfThreads = 1;
            }

            LoadHandlers(node);
        }

        private void LoadHandlers(XElement root)
        {
            _requestHandlers = new List<IJRCommand>();
            var packetHandlers = root.Elements(PacketHandlerConfigurationItem.XML_KEY);
            foreach (var node in packetHandlers)
            {
                var cfg = new PacketHandlerConfigurationItem(node);
                var command = (IJRCommand)Configuration.GetBean(cfg.ClassName);
                command.SetConfig(cfg);
                _requestHandlers.Add(command);
            }

            _eventHandlers = new List<IJRCommand>();
            var eventHandlers = root.Elements(HandlerConfigurationItem.XML_KEY);
            foreach (var node in eventHandlers)
            {
                var cfg = new HandlerConfigurationItem(node);
                var command = (IJRCommand)Configuration.GetBean(cfg.ClassName);
                command.SetConfig(cfg);
                _eventHandlers.Add(command);
            }
        }

        public List<IJRCommand> GetRequestHandlers()
        {
            return _requestHandlers;
        }

        public List<IJRCommand> GetEventHandlers()
        {
            return _eventHandlers;
        }

        public int GetNumberOfThreads()
        {
            return _numberOfThreads;
        }

        public string GetProcessorClassName()
        {
            return _processorClassName;
        }

        public override string XmlKey()
        {
            return XML_KEY;
        }
    }
}
