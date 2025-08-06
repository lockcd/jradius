using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using net.jradius.core.handler;
using net.jradius.core.server.event;

namespace net.jradius.core.server
{
    public class EventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly BlockingCollection<ServerEvent> _eventQueue = new BlockingCollection<ServerEvent>();
        private Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public List<JRCommand> EventHandlers { get; set; }

        public EventDispatcher(ILogger<EventDispatcher> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _task = Task.Run(() => DispatchEvents(_cancellationTokenSource.Token));
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
        }

        public void Dispatch(ServerEvent serverEvent)
        {
            _eventQueue.Add(serverEvent);
        }

        private void DispatchEvents(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var serverEvent = _eventQueue.Take(cancellationToken);
                    if (EventHandlers != null)
                    {
                        foreach (var handler in EventHandlers)
                        {
                            // In a real implementation, we would invoke the handler
                            // handler.Process(serverEvent);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching event");
                }
            }
        }
    }
}
