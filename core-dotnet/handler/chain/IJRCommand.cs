using JRadius.Core.Config;
using JRadius.Core.Server;

namespace JRadius.Core.Handler.Chain
{
    /// <summary>
    /// The JRadius Command Interface for the Chain of Responsibility pattern.
    /// This is the foundation of all handlers within JRadius - which
    /// can be single command, or chains of commands.
    /// </summary>
    public interface IJRCommand : ICommand
    {
        /// <summary>
        /// Set the ConfigurationItem of this handler. All JRadius handlers
        /// have an associated HandlerConfigurationItem associated with it.
        /// </summary>
        /// <param name="cfg">The HandlerConfigurationItem to be set</param>
        void SetConfig(ConfigurationItem cfg);

        /// <summary>
        /// Tests whether or not this handler handles the given JRadiusEvent.
        /// </summary>
        /// <param name="event">The JRadiusEvent (or JRadiusRequest) to be checked</param>
        /// <returns>Returns true if this handler should handle the given event</returns>
        bool DoesHandle(JRadiusEvent @event);

        /// <summary>
        /// The name of the handler (as defined in the configuration)
        /// </summary>
        string Name { get; }
    }
}
