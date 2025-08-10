using JRadius.Core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace JRadius.Core.Impl
{
    public class JRadiusManagerImpl : JRadiusManager, IDisposable
    {
        private readonly ILogger<JRadiusManagerImpl> _logger;
        private bool _startOnLoad = false;
        private EventDispatcher _eventDispatcher;
        private JRadiusServer _jRadiusServer;
        private string _configFile;

        public JRadiusManagerImpl(ILogger<JRadiusManagerImpl> logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _jRadiusServer.Start();
        }

        public void Stop()
        {
            _jRadiusServer.Stop();
        }

        public bool IsRunning()
        {
            return _jRadiusServer.IsRunning();
        }

        public void Initialize()
        {
            if (string.IsNullOrWhiteSpace(_configFile))
            {
                var message = "JRadiusManager: Missing settings filename ['configFile' property not specified correctly].";
                _logger.LogError(message);
                throw new Exception(message);
            }

            // In C#, we can use Assembly.GetExecutingAssembly().GetManifestResourceStream for embedded resources
            // or just File.OpenRead for files on disk. The original code uses ClassLoader.getResourceAsStream,
            // which is more akin to embedded resources. For now, we'll assume a file path.
            try
            {
                using (var stream = new FileStream(_configFile, FileMode.Open, FileAccess.Read))
                {
                    // Configuration.Initialize(stream, null); // TODO: Implement Configuration class
                }
            }
            catch (FileNotFoundException)
            {
                var message = $"File '{_configFile}' not found.";
                _logger.LogError(message);
                throw new Exception(message);
            }

            if (_jRadiusServer == null)
            {
                // _jRadiusServer = new JRadiusServer(_eventDispatcher); // TODO: Implement JRadiusServer constructor
                // _jRadiusServer.AfterPropertiesSet(); // TODO: Implement this method
            }

            if (_startOnLoad)
            {
                _jRadiusServer.Start();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public void SetJRadiusServer(JRadiusServer radiusServer)
        {
            _jRadiusServer = radiusServer;
        }

        public void SetEventDispatcher(EventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public void SetStartOnLoad(bool startOnLoad)
        {
            _startOnLoad = startOnLoad;
        }

        public void SetConfigFile(string configFile)
        {
            _configFile = configFile;
        }
    }
}
