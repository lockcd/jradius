using System.Collections.Generic;

namespace net.jradius.dictionary
{
    public static class AttributeFactory
    {
        private static readonly List<AttributeDictionary> Dictionaries = new List<AttributeDictionary>();

        public static void LoadAttributeDictionary(AttributeDictionary dictionary)
        {
            Dictionaries.Add(dictionary);
        }
    }
}
