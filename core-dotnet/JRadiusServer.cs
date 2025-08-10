using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using net.jradius.core.config;
using net.jradius.core.handler;
using net.jradius.core.session;
using JRadius.Core.Config;

namespace net.jradius.core.server
{
    public class JRadiusServer : IDisposable
    {
        private readonly ILogger<JRadiusServer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EventDispatcher _eventDispatcher;
        private readonly List<Processor> _processors = new List<Processor>();
        private readonly List<Listener> _listeners = new List<Listener>();
        private bool _running;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public JRadiusServer(
            ILogger<JRadiusServer> logger,
            IServiceProvider serviceProvider,
            EventDispatcher eventDispatcher)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _eventDispatcher = eventDispatcher;
        }

        public void Initialize()
        {
            _logger.LogInformation("Initializing JRadius Server....");

            var dictionaryConfigs = Configuration.GetDictionaryConfigs();
            foreach (var dictionaryConfig in dictionaryConfigs)
            {
                _logger.LogInformation($"Loading dictionary: {dictionaryConfig.ClassName}");
                // AttributeFactory.LoadAttributeDictionary((AttributeDictionary)Configuration.GetBean(dictionaryConfig.ClassName));
            }

            var listenerConfigs = Configuration.GetListenerConfigs();
            foreach (var listenerConfig in listenerConfigs)
            {
                var queue = new BlockingCollection<ListenerRequest>();
                CreateListenerWithConfigAndQueue(listenerConfig, queue);
                CreateProcessorsWithConfigAndQueue(listenerConfig, queue);
            }

            _logger.LogInformation("JRadius Server succesfully Initialized.");
        }

        public void Start()
        {
            if (_running) return;

            _logger.LogInformation("Starting Event Dispatcher...");
            _eventDispatcher.Start();

            _logger.LogInformation("Starting Processors...");
            foreach (var processor in _processors)
            {
                processor.Start();
                _logger.LogInformation($"  Started processor {processor.Name}");
            }
            _logger.LogInformation("Processors succesfully started.");

            _logger.LogInformation("Starting Listeners...");
            foreach (var listener in _listeners)
            {
                listener.Start();
                _logger.LogInformation($"  Started listener {listener.Name}");
            }
            _logger.LogInformation("Listeners succesfully started.");

            _running = true;
        }

        public void Stop()
        {
            if (!_running) return;

            _cancellationTokenSource.Cancel();

            foreach (var listener in _listeners)
            {
                _logger.LogInformation($"Stopping listener {listener.Name}");
                listener.Stop();
            }

            foreach (var processor in _processors)
            {
                _logger.LogInformation($"Stopping processor {processor.Name}");
                processor.Stop();
            }

            JRadiusSessionManager.ShutdownManagers();

            _eventDispatcher.Stop();

            _running = false;
        }

        private void CreateProcessorsWithConfigAndQueue(ListenerConfigurationItem listenerConfig, BlockingCollection<ListenerRequest> queue)
        {
            for (var j = 0; j < listenerConfig.NumberOfThreads; j++)
            {
                var processorType = Type.GetType(listenerConfig.ProcessorClassName);
                var processor = (Processor)_serviceProvider.GetService(processorType);
                if (processor == null)
                {
                    throw new InvalidOperationException($"Service not found for type {listenerConfig.ProcessorClassName}");
                }
                processor.RequestQueue = queue;
                _logger.LogInformation($"Created processor {processor.Name}");
                SetPacketHandlersForProcessor(listenerConfig, processor);
                SetEventHandlersForProcessor(listenerConfig, _eventDispatcher);
                processor.EventDispatcher = _eventDispatcher;
                _processors.Add(processor);
            }
        }

        private void SetPacketHandlersForProcessor(ListenerConfigurationItem cfg, Processor processor)
        {
            var requestHandlers = cfg.RequestHandlers;
            if (requestHandlers == null)
            {
                _logger.LogDebug("No packet handlers are configured, maybe using chains instead.");
                return;
            }

            foreach (var handler in requestHandlers)
            {
                _logger.LogInformation($"Packet handler {handler.GetType().FullName}");
            }

            processor.RequestHandlers = requestHandlers;
        }

        private void SetEventHandlersForProcessor(ListenerConfigurationItem cfg, EventDispatcher dispatcher)
        {
            var eventHandlers = cfg.EventHandlers;
            if (eventHandlers == null)
            {
                return;
            }
            foreach (var handler in eventHandlers)
            {
                _logger.LogInformation($"Event handler {handler.GetType().FullName}");
            }

            dispatcher.EventHandlers = eventHandlers;
        }

        private void CreateListenerWithConfigAndQueue(ListenerConfigurationItem listenerConfig, BlockingCollection<ListenerRequest> queue)
        {
            var listenerType = Type.GetType(listenerConfig.ClassName);
            var listener = (Listener)_serviceProvider.GetService(listenerType);
            if (listener == null)
            {
                throw new InvalidOperationException($"Service not found for type {listenerConfig.ClassName}");
            }
            listener.Configuration = listenerConfig;
            listener.RequestQueue = queue;

            _listeners.Add(listener);

            _logger.LogInformation($"Created listener {listener.Name}");
        }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource.Dispose();
        }
    }
}
