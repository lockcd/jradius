using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JRadius.Core.Packet.Attribute
{
    public static class AttributeFactory
    {
        private static Dictionary<long, Type> _attributeMap = new Dictionary<long, Type>();
        private static Dictionary<long, Type> _vendorMap = new Dictionary<long, Type>();
        private static Dictionary<long, VendorValue> _vendorValueMap = new Dictionary<long, VendorValue>();
        private static Dictionary<string, Type> _attributeNameMap = new Dictionary<string, Type>();

        private static readonly ConcurrentDictionary<long, ObjectPool<RadiusAttribute>> _poolCache = new ConcurrentDictionary<long, ObjectPool<RadiusAttribute>>();
        private static readonly DefaultObjectPoolProvider _poolProvider = new DefaultObjectPoolProvider();

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

        public static RadiusAttribute NewAttribute(long key)
        {
            var pool = _poolCache.GetOrAdd(key, k => _poolProvider.Create(new RadiusAttributePooledObjectPolicy(k, _attributeMap, _vendorValueMap)));
            return pool.Get();
        }

        public static void Recycle(RadiusAttribute a)
        {
            if (a == null) return;
            var key = a.GetFormattedType();
            if (_poolCache.TryGetValue(key, out var pool))
            {
                pool.Return(a);
            }
        }
    }

    public class RadiusAttributePooledObjectPolicy : PooledObjectPolicy<RadiusAttribute>
    {
        private readonly long _key;
        private readonly Dictionary<long, Type> _attributeMap;
        private readonly Dictionary<long, AttributeFactory.VendorValue> _vendorValueMap;

        public RadiusAttributePooledObjectPolicy(long key, Dictionary<long, Type> attributeMap, Dictionary<long, AttributeFactory.VendorValue> vendorValueMap)
        {
            _key = key;
            _attributeMap = attributeMap;
            _vendorValueMap = vendorValueMap;
        }

        public override RadiusAttribute Create()
        {
            var vendor = _key >> 16;
            var type = _key & 0xFFFF;

            if (vendor != 0)
            {
                if (_vendorValueMap.TryGetValue(vendor, out var v))
                {
                    if (v.AttributeMap.TryGetValue(type, out var c))
                    {
                        return (RadiusAttribute)Activator.CreateInstance(c);
                    }
                }
                return new Attr_UnknownVSAttribute(vendor, type);
            }
            else
            {
                if (_attributeMap.TryGetValue(type, out var c))
                {
                    return (RadiusAttribute)Activator.CreateInstance(c);
                }
                return new Attr_UnknownAttribute(type);
            }
        }

        public override bool Return(RadiusAttribute obj)
        {
            // TODO: Reset the object state
            return true;
        }
    }
}
