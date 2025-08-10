using JRadius.Core.Config;
using JRadius.Core.Server;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace JRadius.Server
{
    public abstract class TCPListener : IListener
    {
        protected bool _active = false;
        protected ListenerConfigurationItem _config;
        protected BlockingCollection<ListenerRequest> _queue;
        protected int _port = 1814;
        protected int _backlog = 1024;
        protected bool _requiresSSL = false;
        protected bool _usingSSL = false;
        protected bool _keepAlive;
        protected TcpListener _serverSocket;
        protected readonly List<KeepAliveListener> _keepAliveListeners = new List<KeepAliveListener>();

        protected bool _sslWantClientAuth;
        protected bool _sslNeedClientAuth;
        protected SslProtocols _sslEnabledProtocols;

        public void SetConfiguration(ListenerConfigurationItem cfg)
        {
            SetConfiguration(cfg, false);
        }

        public void SetConfiguration(ListenerConfigurationItem cfg, bool noKeepAlive)
        {
            _keepAlive = !noKeepAlive;
            _config = cfg;
            var props = _config.Properties;
            if (props.TryGetValue("port", out var s)) _port = int.Parse(s);
            if (props.TryGetValue("backlog", out s)) _backlog = int.Parse(s);
            if (_keepAlive)
            {
                if (props.TryGetValue("keepAlive", out s)) _keepAlive = bool.Parse(s);
            }

            if (props.TryGetValue("useSSL", out var useSSL) && bool.Parse(useSSL) || _requiresSSL)
            {
                // TODO: Implement SSL configuration
                _usingSSL = true;
            }
            else
            {
                _serverSocket = new TcpListener(IPAddress.Any, _port);
            }

            _active = true;
        }

        public void SetRequestQueue(BlockingCollection<ListenerRequest> q)
        {
            _queue = q;
        }

        public void SetListenerConfigurationItem(ListenerConfigurationItem cfg)
        {
            _config = cfg;
        }

        public async Task Listen()
        {
            _serverSocket.Start(_backlog);
            while (_active)
            {
                var socket = await _serverSocket.AcceptSocketAsync();
                if (_keepAlive)
                {
                    var keepAliveListener = new KeepAliveListener(socket, this, _queue);
                    keepAliveListener.Start();
                    lock (_keepAliveListeners)
                    {
                        _keepAliveListeners.Add(keepAliveListener);
                    }
                }
                else
                {
                    // TODO: Implement non-keep-alive mode
                }
            }
        }

        internal void DeadKeepAliveListener(KeepAliveListener keepAliveListener)
        {
            lock (_keepAliveListeners)
            {
                _keepAliveListeners.Remove(keepAliveListener);
            }
        }

        public void SetActive(bool active)
        {
            _active = active;
            if (!active)
            {
                _serverSocket.Stop();
                lock (_keepAliveListeners)
                {
                    foreach (var listener in _keepAliveListeners)
                    {
                        listener.Shutdown(true);
                    }
                    _keepAliveListeners.Clear();
                }
            }
        }

        public void Run()
        {
            Listen().Wait();
        }
    }
}
