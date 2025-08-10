using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using net.jradius.core.server;
using net.jradius.core.session;
using System;
using System.Collections.Generic;

namespace JRadius.Core.Session
{
    public class JRadiusSessionManager : IDisposable
    {
        private static JRadiusSessionManager _defaultManager;
        private static Dictionary<string, JRadiusSessionManager> _managers = new Dictionary<string, JRadiusSessionManager>();

        private Dictionary<string, ISessionKeyProvider> _providers = new Dictionary<string, ISessionKeyProvider>();
        private Dictionary<string, ISessionFactory> _factories = new Dictionary<string, ISessionFactory>();

        private int _minInterimInterval = 300;
        private int _maxInactiveInterval = 2100;

        private IMemoryCache _sessionCache;
        private IMemoryCache _logCache;

        private EventDispatcher _eventDispatcher;
        private readonly ILogger<JRadiusSessionManager> _logger;

        public JRadiusSessionManager(ILogger<JRadiusSessionManager> logger, IMemoryCache sessionCache, IMemoryCache logCache)
        {
            _logger = logger;
            _sessionCache = sessionCache;
            _logCache = logCache;
            Initialize();
        }

        private void Initialize()
        {
            // In a real implementation, we would use reflection to find and instantiate
            // the default providers and factories, or use dependency injection.
            // For now, we'll leave this empty.
        }

        public static JRadiusSessionManager GetManager(object name)
        {
            JRadiusSessionManager manager = null;
            if (name != null)
            {
                _managers.TryGetValue(name.ToString(), out manager);
            }

            if (manager == null)
            {
                if (_defaultManager == null)
                {
                    // This is not ideal. In a real application, the default manager should be
                    // configured and created by the application's startup code.
                    // _defaultManager = new JRadiusSessionManager( ... );
                }
                manager = _defaultManager;
            }
            return manager;
        }

        public static JRadiusSessionManager SetManager(string name, JRadiusSessionManager manager)
        {
            if (name != null)
            {
                _managers[name] = manager;
            }
            else
            {
                _defaultManager = manager;
            }
            return manager;
        }

        public static void ShutdownManagers()
        {
            if (_defaultManager != null)
            {
                _defaultManager.Dispose();
            }

            foreach (var manager in _managers.Values)
            {
                manager.Dispose();
            }
        }

        public void Dispose()
        {
            _sessionCache.Dispose();
            _logCache.Dispose();
        }

        public void SetSessionKeyProvider(string name, ISessionKeyProvider provider)
        {
            _providers[name] = provider;
        }

        public void SetSessionFactory(string name, ISessionFactory factory)
        {
            _factories[name] = factory;
        }

        public ISessionKeyProvider GetSessionKeyProvider(object name)
        {
            _providers.TryGetValue(name?.ToString(), out ISessionKeyProvider provider);
            if (provider == null && name != null)
            {
                _providers.TryGetValue(null, out provider);
            }
            return provider;
        }

        public ISessionFactory GetSessionFactory(object name)
        {
            _factories.TryGetValue(name?.ToString(), out ISessionFactory factory);
            if (factory == null && name != null)
            {
                _factories.TryGetValue(null, out factory);
            }
            return factory;
        }

        public JRadiusSession GetSession(JRadiusRequest request)
        {
            // TODO: Implement the session retrieval logic.
            return null;
        }

        public void RemoveSession(JRadiusSession session)
        {
            if (session != null)
            {
                _sessionCache.Remove(session.JRadiusKey);
                _sessionCache.Remove(session.SessionKey);
            }
        }
    }
}
