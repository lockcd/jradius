using System.Collections.Generic;
using net.jradius.core.handler;

namespace net.jradius.core.config
{
    public class ListenerConfigurationItem
    {
        public string ClassName { get; set; }
        public int NumberOfThreads { get; set; }
        public string ProcessorClassName { get; set; }
        public List<JRCommand> RequestHandlers { get; set; }
        public List<JRCommand> EventHandlers { get; set; }
    }
}
