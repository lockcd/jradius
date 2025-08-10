using JRadius.Core.Handler.Chain;
using JRadius.Core.Server;
using Microsoft.Extensions.Logging;
using net.jradius.core.server;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace net.jradius.core
{
    public abstract class Processor
    {
        public string Name { get; set; }
        public BlockingCollection<ListenerRequest> RequestQueue { get; set; }
        public List<IJRCommand> RequestHandlers { get; set; }
        public EventDispatcher EventDispatcher { get; set; }

        protected readonly ILogger<Processor> _logger;
        private Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected Processor(ILogger<Processor> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _task = Task.Run(() => ProcessRequests(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
        }

        protected abstract void ProcessRequests(CancellationToken cancellationToken);
    }
}
