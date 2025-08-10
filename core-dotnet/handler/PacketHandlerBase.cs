using JRadius.Core.Config;
using JRadius.Core.Server;

namespace JRadius.Core.Handler
{
    public abstract class PacketHandlerBase : IJRCommand
    {
        protected ConfigurationItem _config;

        public string Name => _config.Name;

        public virtual bool DoesHandle(JRadiusEvent @event)
        {
            return true;
        }

        public virtual bool Execute(Chain.IContext context)
        {
            if (context is JRadiusRequest jRequest)
            {
                return Handle(jRequest);
            }
            return false;
        }

        public abstract bool Handle(JRadiusRequest jRequest);

        public virtual void SetConfig(ConfigurationItem cfg)
        {
            _config = cfg;
        }
    }
}
