using System.Collections.Generic;

namespace JRadius.Core.Handler.Chain
{
    public class CatalogBase
    {
        protected Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public void AddCommand(string name, ICommand command)
        {
            _commands[name] = command;
        }

        public ICommand GetCommand(string name)
        {
            _commands.TryGetValue(name, out ICommand command);
            return command;
        }
    }
}
