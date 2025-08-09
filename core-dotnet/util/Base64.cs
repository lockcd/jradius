using System;

namespace JRadius.Core.Util
{
    public class Base64
    {
        public static string Encode(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            return Convert.ToBase64String(data);
        }

        public static byte[] Decode(string base64String)
        {
            if (base64String == null)
            {
                return null;
            }
            return Convert.FromBase64String(base64String);
        }
    }
}
