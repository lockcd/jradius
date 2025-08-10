using System;
using System.Collections.Generic;

namespace JRadius.Core.Packet.Attribute
{
    public interface IVSADictionary
    {
        string GetVendorName();
        void LoadAttributes(Dictionary<long, Type> map);
        void LoadAttributesNames(Dictionary<string, Type> map);
    }
}
