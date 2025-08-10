using System.Security.Cryptography;

namespace JRadius.Core.Util
{
    public static class MD5
    {
        public static byte[] HmacMd5(byte[] data, int offset, int length, byte[] key)
        {
            using (var hmac = new HMACMD5(key))
            {
                return hmac.ComputeHash(data, offset, length);
            }
        }
    }
}
