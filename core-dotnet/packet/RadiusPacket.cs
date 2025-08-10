using JRadius.Core.Packet.Attribute;
using System;
using System.Text;
using System.Threading;

namespace JRadius.Core.Packet
{
    public abstract class RadiusPacket
    {
        public const int MIN_PACKET_LENGTH = 20;
        public const int MAX_PACKET_LENGTH = 4096;
        public const short RADIUS_HEADER_LENGTH = 20;

        private static int _nextPacketId = 1;
        private static readonly object _nextPacketIdLock = new object();

        protected int _code;
        protected int _identifier = -1;
        protected byte[] _authenticator;

        protected readonly AttributeList _attributes = new AttributeList();

        protected bool _recyclable;
        protected bool _recycled;

        public RadiusPacket()
        {
        }

        public RadiusPacket(AttributeList list)
        {
            if (list != null)
            {
                _attributes.Copy(list, _recyclable);
            }
        }

        public void SetCode(int code)
        {
            _code = code;
        }

        public int GetCode()
        {
            return _code;
        }

        public void AddAttribute(RadiusAttribute attribute)
        {
            if (null != attribute)
            {
                _attributes.Add(attribute, false);
            }
        }

        public void OverwriteAttribute(RadiusAttribute attribute)
        {
            if (null != attribute)
            {
                _attributes.Add(attribute, true);
            }
        }

        public void AddAttributes(AttributeList list)
        {
            _attributes.Add(list);
        }

        public void RemoveAttribute(RadiusAttribute attribute)
        {
            _attributes.Remove(attribute);
        }

        public void RemoveAttribute(long attributeType)
        {
            _attributes.Remove(attributeType);
        }

        public int GetIdentifier()
        {
            if (_identifier < 0)
            {
                _identifier = GetNewPacketId();
            }
            return _identifier;
        }

        public void SetIdentifier(int i)
        {
            _identifier = i;
        }

        public AttributeList GetAttributes()
        {
            return _attributes;
        }

        public virtual byte[] CreateAuthenticator(byte[] attributes, int offset, int attributesLength, string sharedSecret)
        {
            return new byte[16];
        }

        public virtual bool VerifyAuthenticator(string sharedSecret)
        {
            return false;
        }

        public void SetAuthenticator(byte[] authenticator)
        {
            _authenticator = authenticator;
        }

        public byte[] GetAuthenticator()
        {
            return _authenticator;
        }

        public RadiusAttribute FindAttribute(long type)
        {
            return (RadiusAttribute)_attributes.Get(type, true);
        }

        public object GetAttributeValue(long type)
        {
            var attr = FindAttribute(type);
            return attr?.GetValue().GetValueObject();
        }

        private static int GetNewPacketId()
        {
            lock (_nextPacketIdLock)
            {
                _nextPacketId = (_nextPacketId + 1) % 255;
                return _nextPacketId;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Class: {GetType()}");
            sb.AppendLine("Attributes:");
            sb.AppendLine(_attributes.ToString());
            return sb.ToString();
        }

        public bool IsRecyclable()
        {
            return _recyclable;
        }
    }
}
