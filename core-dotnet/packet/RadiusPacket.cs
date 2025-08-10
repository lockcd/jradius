using System;
using System.Text;
using JRadius.Core.Packet.Attribute;
using net.jradius.core.packet.attribute;
using net.jradius.core.packet.attribute.value;

namespace net.jradius.core.packet
{
    public abstract class RadiusPacket
    {
        private static readonly object NextPacketIdLock = new object();
        private static int _nextPacketId = 1;

        public const int MinPacketLength = 20;
        public const int MaxPacketLength = 4096;
        public const short RadiusHeaderLength = 20;

        protected int code;
        protected int identifier = -1;
        protected byte[] authenticator;

        protected readonly AttributeList attributes = new AttributeList();

        protected bool recyclable;
        protected bool recycled;

        public RadiusPacket()
        {
        }

        public RadiusPacket(AttributeList list)
        {
            if (list != null)
            {
                attributes.Copy(list, recyclable);
            }
        }

        public void SetCode(int code)
        {
            this.code = (byte)code;
        }

        public int Code
        {
            get { return code; }
        }

        public void AddAttribute(RadiusAttribute attribute)
        {
            if (null != attribute)
            {
                attributes.Add(attribute, false);
            }
        }

        public void OverwriteAttribute(RadiusAttribute attribute)
        {
            if (null != attribute)
            {
                attributes.Add(attribute, true);
            }
        }

        public void AddAttributes(AttributeList list)
        {
            attributes.Add(list);
        }

        public void RemoveAttribute(RadiusAttribute attribute)
        {
            attributes.Remove(attribute);
        }

        public void RemoveAttribute(long attributeType)
        {
            attributes.Remove(attributeType);
        }

        public int Identifier
        {
            get
            {
                if (this.identifier < 0)
                {
                    this.identifier = NewPacketId;
                }
                return this.identifier;
            }
            set { this.identifier = value; }
        }

        public AttributeList Attributes
        {
            get { return attributes; }
        }

        public virtual byte[] CreateAuthenticator(byte[] attributes, int offset, int attributsLength, String sharedSecret)
        {
            return new byte[16];
        }

        public virtual bool VerifyAuthenticator(String sharedSecret)
        {
            return false;
        }

        public byte[] Authenticator
        {
            get { return this.authenticator; }
            set { this.authenticator = value; }
        }

        public byte[] GetAuthenticator(byte[] attributes, String sharedSecret)
        {
            if (this.authenticator == null)
            {
                if (sharedSecret != null)
                    this.authenticator = CreateAuthenticator(attributes, 0, attributes.Length, sharedSecret);
                else
                    this.authenticator = new byte[16];
            }

            return this.authenticator;
        }

        public byte[] GetAuthenticator(byte[] attributes, int offset, int attributesLength, String sharedSecret)
        {
            if (this.authenticator == null)
            {
                if (sharedSecret != null)
                    this.authenticator = CreateAuthenticator(attributes, offset, attributesLength, sharedSecret);
                else
                    this.authenticator = new byte[16];
            }

            return this.authenticator;
        }

        public RadiusAttribute FindAttribute(long type)
        {
            return attributes.Get(type);
        }

        public object[] FindAttributes(long type)
        {
            return attributes.GetArray(type);
        }

        public RadiusAttribute FindAttribute(String aName)
        {
            return attributes.Get(aName);
        }

        public object GetAttributeValue(long type)
        {
            return attributes.GetValue(type);
        }

        public object GetAttributeValue(String aName)
        {
            RadiusAttribute attribute = FindAttribute(aName);
            if (attribute != null)
            {
                AttributeValue value = attribute.Value;
                if (value != null)
                {
                    return value.ValueObject;
                }
            }
            return null;
        }

        private static int NewPacketId
        {
            get
            {
                lock (NextPacketIdLock)
                {
                    _nextPacketId = (_nextPacketId + 1) % 255;
                    return _nextPacketId;
                }
            }
        }

        public string ToString(bool nonStandardAtts, bool unknownAttrs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Class: ").Append(this.GetType().ToString()).Append("\n");
            sb.Append("Attributes:\n");
            sb.Append(attributes.ToString(nonStandardAtts, unknownAttrs));
            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(true, true);
        }

        public bool IsRecyclable
        {
            get { return recyclable; }
        }
    }
}
