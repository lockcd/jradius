using System.Collections.Generic;

namespace JRadius.Core.Handler.Chain
{
    public class ChainBase : ICommand
    {
        protected List<ICommand> _commands = new List<ICommand>();

        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }

        public virtual bool Execute(IContext context)
        {
            foreach (var command in _commands)
            {
                if (command.Execute(context))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
