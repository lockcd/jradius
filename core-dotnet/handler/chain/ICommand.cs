namespace JRadius.Core.Handler.Chain
{
    public interface ICommand
    {
        /// <summary>
        /// Execute a unit of processing work to be performed.
        /// </summary>
        /// <param name="context">The context for this command.</param>
        /// <returns>
        /// Return true if processing of this command completes, and processing of the chain should be terminated.
        /// Return false if processing of this command completes, and processing of the chain should continue.
        /// </returns>
        bool Execute(IContext context);
    }
}
