using System;
using System.IO;
using System.Xml.Linq;

namespace JRadius.Core.Handler.Chain
{
    public class JRConfigParser
    {
        public JRCatalogBase Parse(Stream stream)
        {
            var catalog = new JRCatalogBase();
            var doc = XDocument.Load(stream);

            foreach (var defineElement in doc.Root.Elements("define"))
            {
                var name = defineElement.Attribute("name")?.Value;
                var className = defineElement.Attribute("className")?.Value;
                if (name != null && className != null)
                {
                    // For now, we will just store the class name.
                    // The command will be created when it is requested.
                    catalog.AddCommand(name, new LazyCommand(className));
                }
            }

            foreach (var chainElement in doc.Root.Elements("chain"))
            {
                var chain = new JRChainBase(null); // TODO: Inject logger
                var chainName = chainElement.Attribute("name")?.Value;
                chain.Name = chainName;

                foreach (var commandElement in chainElement.Elements())
                {
                    var className = commandElement.Attribute("className")?.Value;
                    if (className != null)
                    {
                        var command = (ICommand)Activator.CreateInstance(Type.GetType(className));
                        chain.AddCommand(command);
                    }
                }
                catalog.AddCommand(chainName, chain);
            }
            return catalog;
        }
    }

    public class LazyCommand : ICommand
    {
        private readonly string _className;
        private ICommand _command;

        public LazyCommand(string className)
        {
            _className = className;
        }

        public bool Execute(IContext context)
        {
            if (_command == null)
            {
                _command = (ICommand)Activator.CreateInstance(Type.GetType(_className));
            }
            return _command.Execute(context);
        }
    }
}
