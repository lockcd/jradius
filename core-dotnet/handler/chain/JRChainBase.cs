using JRadius.Core.Config;
using JRadius.Core.Server;
using Microsoft.Extensions.Logging;

namespace JRadius.Core.Handler.Chain
{
    public class JRChainBase : ChainBase, IJRCommand
    {
        private readonly ILogger<JRChainBase> _logger;
        protected HandlerConfigurationItem _config;

        public JRChainBase(ILogger<JRChainBase> logger)
        {
            _logger = logger;
        }

        public string Name { get; set; }

        public void SetConfig(ConfigurationItem cfg)
        {
            _config = (HandlerConfigurationItem)cfg;
        }

        public bool DoesHandle(JRadiusEvent @event)
        {
            if (_config == null) return true;
            return (_config.HandlesSender(@event.GetSender()) &&
                    _config.HandlesType(@event.GetTypeString()));
        }

        public override bool Execute(IContext context)
        {
            _logger.LogDebug($"Executing command: {Name}");
            return base.Execute(context);
        }

        public void Initialize()
        {
            foreach (var command in _commands)
            {
                // In a real DI scenario, we would resolve dependencies here.
                // For now, we assume commands are already configured.
                if (command is JRChainBase chain)
                {
                    chain.Initialize();
                }
            }
        }
    }
}
