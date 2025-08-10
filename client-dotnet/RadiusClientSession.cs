using JRadius.Core.Client.Auth;
using JRadius.Core.Packet.Attribute;
using System;
using System.Threading;

namespace JRadius.Core.Client
{
    public class RadiusClientSession : IDisposable
    {
        private RadiusClient _radiusClient;

        private long _octetsIn;
        private long _octetsOut;
        private long _packetsIn;
        private long _packetsOut;
        private long _sessionTime;

        private long _idleTimeout;
        private long _sessionTimeout;
        private long _interimInterval;

        private bool _authenticated = false;
        private bool _stopped = false;
        private RadiusAttribute _classAttribute;
        private IRadiusAuthenticator _radiusAuthenticator;
        private Timer _timer;

        public void Run()
        {
            // This will be called by the timer
        }

        public void Start()
        {
            if (_authenticated)
            {
                _timer = new Timer(s => Run(), null, 0, _interimInterval * 1000);
            }
        }

        public void Stop()
        {
            _stopped = true;
            _timer?.Dispose();
        }

        public void Dispose()
        {
            Stop();
        }

        public void IncrementOctetsIn(long l)
        {
            Interlocked.Add(ref _octetsIn, l);
        }

        public void IncrementOctetsOut(long l)
        {
            Interlocked.Add(ref _octetsOut, l);
        }

        public void IncrementPacketsIn(long l)
        {
            Interlocked.Add(ref _packetsIn, l);
        }

        public void IncrementPacketsOut(long l)
        {
            Interlocked.Add(ref _packetsOut, l);
        }

        public class RadiusClientSessionException : Exception
        {
            public RadiusClientSessionException(string s)
                : base(s)
            {
            }
        }

        // Getters and Setters
        public RadiusAttribute GetClassAttribute()
        {
            return _classAttribute;
        }

        public void SetClassAttribute(RadiusAttribute classAttribute)
        {
            _classAttribute = classAttribute;
        }

        public long GetIdleTimeout()
        {
            return _idleTimeout;
        }

        public void SetIdleTimeout(long idleTimeout)
        {
            _idleTimeout = idleTimeout;
        }

        public long GetInterimInterval()
        {
            return _interimInterval;
        }

        public void SetInterimInterval(long interimInterval)
        {
            _interimInterval = interimInterval;
        }

        public long GetOctetsIn()
        {
            return _octetsIn;
        }

        public void SetOctetsIn(long octetsIn)
        {
            _octetsIn = octetsIn;
        }

        public long GetOctetsOut()
        {
            return _octetsOut;
        }

        public void SetOctetsOut(long octetsOut)
        {
            _octetsOut = octetsOut;
        }

        public long GetPacketsIn()
        {
            return _packetsIn;
        }

        public void SetPacketsIn(long packetsIn)
        {
            _packetsIn = packetsIn;
        }

        public long GetPacketsOut()
        {
            return _packetsOut;
        }

        public void SetPacketsOut(long packetsOut)
        {
            _packetsOut = packetsOut;
        }

        public IRadiusAuthenticator GetRadiusAuthenticator()
        {
            return _radiusAuthenticator;
        }

        public void SetRadiusAuthenticator(IRadiusAuthenticator radiusAuthenticator)
        {
            _radiusAuthenticator = radiusAuthenticator;
        }

        public RadiusClient GetRadiusClient()
        {
            return _radiusClient;
        }

        public void SetRadiusClient(RadiusClient radiusClient)
        {
            _radiusClient = radiusClient;
        }

        public long GetSessionTime()
        {
            return _sessionTime;
        }

        public void SetSessionTime(long sessionTime)
        {
            _sessionTime = sessionTime;
        }

        public long GetSessionTimeout()
        {
            return _sessionTimeout;
        }

        public void SetSessionTimeout(long sessionTimeout)
        {
            _sessionTimeout = sessionTimeout;
        }
    }
}
