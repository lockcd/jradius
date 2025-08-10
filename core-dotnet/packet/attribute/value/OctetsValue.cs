using JRadius.Core.Util;
using System;
using System.IO;

namespace JRadius.Core.Packet.Attribute.Value
{
    public class OctetsValue : IAttributeValue
    {
        protected byte[] _byteValue;
        protected int _byteValueOffset;
        protected int _byteValueLength;

        public OctetsValue() { }

        public OctetsValue(byte[] b)
        {
            _byteValue = b;
            _byteValueLength = b?.Length ?? 0;
        }

        public void Copy(IAttributeValue value)
        {
            var cValue = (OctetsValue)value;
            _byteValueLength = cValue._byteValueLength;
            _byteValue = new byte[_byteValueLength];
            _byteValueOffset = 0;
            if (_byteValueLength > 0)
            {
                System.Array.Copy(cValue._byteValue, cValue._byteValueOffset, _byteValue, 0, _byteValueLength);
            }
        }

        public void GetBytes(Stream out_Renamed)
        {
            if (_byteValue != null)
            {
                out_Renamed.Write(_byteValue, _byteValueOffset, _byteValueLength);
            }
        }

        public byte[] GetBytes()
        {
            if (_byteValueLength == 0) return new byte[0];
            var ret = new byte[_byteValueLength];
            System.Array.Copy(_byteValue, _byteValueOffset, ret, 0, _byteValueLength);
            return ret;
        }

        public int GetLength()
        {
            return _byteValueLength;
        }

        public object GetValueObject()
        {
            return GetBytes();
        }

        public void SetValue(byte[] b)
        {
            _byteValue = b;
            _byteValueOffset = 0;
            _byteValueLength = b?.Length ?? 0;
        }

        public void SetValue(byte[] b, int off, int len)
        {
            _byteValue = b;
            _byteValueOffset = off;
            _byteValueLength = len;
        }

        public void SetValue(string s)
        {
            if (s.StartsWith("0x"))
            {
                SetValue(Hex.HexStringToByteArray(s.Substring(2)));
            }
            else
            {
                SetValue(System.Text.Encoding.UTF8.GetBytes(s));
            }
        }

        public void SetValueObject(object o)
        {
            if (o is byte[] bytes)
            {
                SetValue(bytes);
            }
            else
            {
                SetValue(o.ToString());
            }
        }

        public string ToDebugString()
        {
            return $"[Binary Data: {(_byteValue == null ? "null" : "0x" + Hex.ByteArrayToHexString(GetBytes()))}]";
        }

        public override string ToString()
        {
            return $"[Binary Data (length={(_byteValue == null ? 0 : _byteValueLength)})]";
        }

        public string ToXMLString()
        {
            return "";
        }
    }
}
