using System.Collections.Generic;
using System.Xml.Linq;

namespace JRadius.Core.Config
{
    public class ListenerConfigurationItem : ConfigurationItem
    {
        public static string XML_LIST_KEY = "listeners";
        public static string XML_KEY = "listener";

        private List<object> _requestHandlers; // TODO: Replace object with JRCommand
        private List<object> _eventHandlers; // TODO: Replace object with JRCommand
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

            // TODO: Implement the handler loading logic here.
            // This requires the chain of responsibility pattern and the getBean method to be implemented.
        }

        public List<object> GetRequestHandlers()
        {
            return _requestHandlers;
        }

        public List<object> GetEventHandlers()
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
