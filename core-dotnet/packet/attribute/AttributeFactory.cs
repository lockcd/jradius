using System;
using System.Collections.Generic;

namespace JRadius.Core.Packet.Attribute
{
    public static class AttributeFactory
    {
        private static Dictionary<long, Type> _attributeMap = new Dictionary<long, Type>();
        private static Dictionary<long, Type> _vendorMap = new Dictionary<long, Type>();
        private static Dictionary<long, VendorValue> _vendorValueMap = new Dictionary<long, VendorValue>();
        private static Dictionary<string, Type> _attributeNameMap = new Dictionary<string, Type>();

        public sealed class VendorValue
        {
            public Type DictClass { get; }
            public Dictionary<long, Type> AttributeMap { get; }
            public Dictionary<string, Type> AttributeNameMap { get; }

            public VendorValue(Type c, Dictionary<long, Type> t, Dictionary<string, Type> n)
            {
                DictClass = c;
                AttributeMap = t;
                AttributeNameMap = n;
            }
        }

        public static bool LoadAttributeDictionary(string className)
        {
            try
            {
                var clazz = Type.GetType(className);
                var o = Activator.CreateInstance(clazz);
                return LoadAttributeDictionary((IAttributeDictionary)o);
            }
            catch (Exception e)
            {
                // In a real application, we would use a logger here.
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool LoadAttributeDictionary(IAttributeDictionary dict)
        {
            dict.LoadAttributes(_attributeMap);
            dict.LoadAttributesNames(_attributeNameMap);
            dict.LoadVendorCodes(_vendorMap);

            foreach (var id in _vendorMap.Keys)
            {
                var c = _vendorMap[id];
                try
                {
                    var typeMap = new Dictionary<long, Type>();
                    var nameMap = new Dictionary<string, Type>();
                    var vsadict = (IVSADictionary)Activator.CreateInstance(c);
                    vsadict.LoadAttributes(typeMap);
                    vsadict.LoadAttributesNames(nameMap);
                    foreach (var name in nameMap.Keys)
                    {
                        _attributeNameMap[name] = nameMap[name];
                    }
                    _vendorValueMap[id] = new VendorValue(c, typeMap, nameMap);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return true;
        }

        // TODO: Implement the rest of the class, including the object pooling mechanism.
    }
}
