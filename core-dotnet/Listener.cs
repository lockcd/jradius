using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using JRadius.Core.Config;

namespace JRadius.Core.Server
{
    public abstract class Listener
    {
        public string Name { get; set; }
        public ListenerConfigurationItem Configuration { get; set; }
        public BlockingCollection<ListenerRequest> RequestQueue { get; set; }

        protected readonly ILogger<Listener> _logger;
        private Task _task;

        protected Listener(ILogger<Listener> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _task = Task.Run(() => Listen());
        }

        public void Stop()
        {
            // How to stop the listener will depend on the implementation
        }

        protected abstract void Listen();
    }
}
