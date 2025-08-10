using System.IO;

namespace JRadius.Core.Packet.Attribute.Value
{
    public interface IAttributeValue
    {
        void GetBytes(Stream io);
        byte[] GetBytes();
        int GetLength();
        object GetValueObject();
        void SetValue(byte[] b);
        void SetValue(byte[] b, int off, int len);
        void SetValueObject(object o);
        void Copy(IAttributeValue value);
        void SetValue(string s);
        string ToDebugString();
        string ToXMLString();
    }
}
