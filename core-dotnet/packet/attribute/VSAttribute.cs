using System;

namespace JRadius.Core.Packet.Attribute
{
    public abstract class VSAttribute : RadiusAttribute
    {
        protected long _vendorId;
        protected long _vsaAttributeType;
        protected short _typeLength = 1;
        protected short _lengthLength = 1;
        protected short _extraLength = 0;
        protected bool _hasContinuationByte;
        protected short _continuation;
        protected bool _grouped = false;

        public void SetFormat(string format)
        {
            var s = format.Split(',');
            if (s != null && s.Length > 0)
            {
                _typeLength = short.Parse(s[0]);
                if (s.Length > 1)
                {
                    _lengthLength = short.Parse(s[1]);
                }
                if (s.Length > 2)
                {
                    if (s[2] == "c")
                    {
                        _hasContinuationByte = true;
                    }
                }
            }
        }

        public override long GetFormattedType()
        {
            return _vsaAttributeType | (_vendorId << 16);
        }

        public long GetVendorId()
        {
            return _vendorId;
        }

        public void SetVendorId(long vendorId)
        {
            _vendorId = vendorId;
        }

        public long GetVsaAttributeType()
        {
            return _vsaAttributeType;
        }

        public void SetVsaAttributeType(long vsaAttributeType)
        {
            _vsaAttributeType = vsaAttributeType;
        }

        public short GetTypeLength()
        {
            return _typeLength;
        }

        public short GetLengthLength()
        {
            return _lengthLength;
        }

        public short GetExtraLength()
        {
            return _extraLength;
        }

        public bool HasContinuationByte()
        {
            return _hasContinuationByte;
        }

        public int GetContinuation()
        {
            return _continuation;
        }

        public void SetContinuation(short cont)
        {
            _continuation = cont;
        }

        public void SetContinuation()
        {
            SetContinuation((short)(1 << 7));
        }

        public void UnsetContinuation()
        {
            SetContinuation((short)0);
        }

        public bool IsGrouped()
        {
            return _grouped;
        }

        public void SetGrouped(bool grouped)
        {
            _grouped = grouped;
        }
    }
}
