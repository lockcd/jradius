using JRadius.Core.Config;
using JRadius.Core.Server;
using System.Collections.Concurrent;

namespace JRadius.Server
{
    public interface IListener
    {
        void SetConfiguration(ListenerConfigurationItem cfg);
        void SetRequestQueue(BlockingCollection<ListenerRequest> q);
        void SetListenerConfigurationItem(ListenerConfigurationItem cfg);
        void SetActive(bool active);
        void Run();
    }
}
