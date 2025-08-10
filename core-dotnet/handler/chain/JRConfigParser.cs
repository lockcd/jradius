using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace JRadius.Core.Handler.Chain
{
    public class JRConfigParser
    {
        public JRCatalogBase Parse(Stream stream)
        {
            var catalog = new JRCatalogBase();
            var doc = XDocument.Load(stream);

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
}
