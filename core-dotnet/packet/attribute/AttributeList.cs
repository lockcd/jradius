using System;
using System.Collections.Generic;
using System.Linq;

namespace JRadius.Core.Packet.Attribute
{
    public class AttributeList
    {
        private List<RadiusAttribute> _attributeOrderList = new List<RadiusAttribute>();
        private Dictionary<long, object> _attributeMap = new Dictionary<long, object>();

        public void Add(AttributeList list)
        {
            Copy(list, true);
        }

        public void Copy(AttributeList list, bool pool)
        {
            if (list != null)
            {
                foreach (var a in list.GetAttributeList())
                {
                    // TODO: Implement pooling
                    _Add(a, false);
                }
            }
        }

        public void Add(RadiusAttribute a)
        {
            Add(a, true);
        }

        private void _Add(RadiusAttribute a, bool overwrite)
        {
            var key = a.GetFormattedType();
            var o = _attributeMap.GetValueOrDefault(key);
            _attributeOrderList.Add(a);

            if (o == null || overwrite)
            {
                Remove(key);
                _attributeMap[key] = a;
            }
            else
            {
                if (o is List<RadiusAttribute> list)
                {
                    list.Add(a);
                }
                else
                {
                    var newList = new List<RadiusAttribute> { (RadiusAttribute)o, a };
                    _attributeMap[key] = newList;
                }
            }
        }

        public void Add(RadiusAttribute a, bool overwrite)
        {
            if (a is SubAttribute subAttribute)
            {
                try
                {
                    var parentType = subAttribute.ParentClass;
                    var pAttribute = (RadiusAttribute)Get(parentType.GetProperty("FormattedType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null, null), true);

                    if (pAttribute == null)
                    {
                        pAttribute = (RadiusAttribute)Activator.CreateInstance(parentType);
                        Add(pAttribute);
                    }
                    ((VSAWithSubAttributes)pAttribute).GetSubAttributes()._Add(a, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                _Add(a, overwrite);
            }
        }

        public void Remove(RadiusAttribute a)
        {
            if (a != null)
            {
                Remove(a.GetFormattedType());
            }
        }

        public void Remove(long attributeType)
        {
            var key = attributeType;
            if (_attributeMap.Remove(key, out var o))
            {
                if (o is List<RadiusAttribute> list)
                {
                    foreach (var item in list)
                    {
                        RemoveFromList(item);
                    }
                }
                else
                {
                    RemoveFromList(o);
                }
            }
        }

        public void Clear()
        {
            // TODO: Implement pooling
            _attributeMap.Clear();
            _attributeOrderList.Clear();
        }

        private void RemoveFromList(object o)
        {
            for (int i = 0; i < _attributeOrderList.Count; i++)
            {
                if (_attributeOrderList[i] == o)
                {
                    _attributeOrderList.RemoveAt(i);
                    // TODO: Implement pooling
                    return;
                }
            }
        }

        public int GetSize()
        {
            return _attributeOrderList.Count;
        }

        public object Get(long type, bool single)
        {
            if (_attributeMap.TryGetValue(type, out var o))
            {
                if (o is List<RadiusAttribute> list)
                {
                    return single ? list.FirstOrDefault() : list;
                }
                return o;
            }
            return null;
        }

        public List<RadiusAttribute> GetAttributeList()
        {
            return _attributeOrderList;
        }
    }
}
