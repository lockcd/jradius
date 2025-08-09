using System.Security.Cryptography;
using System.Text;

namespace JRadius.Core.Util
{
    public class MD5
    {
        public static byte[] GetHash(byte[] input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return md5.ComputeHash(input);
            }
        }

        public static byte[] GetHash(string input)
        {
            return GetHash(Encoding.UTF8.GetBytes(input));
        }

        public static byte[] HmacMd5(byte[] key, byte[] data)
        {
            using (var hmac = new HMACMD5(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        public static byte[] HmacSha1(byte[] key, byte[] data)
        {
            using (var hmac = new HMACSHA1(key))
            {
                return hmac.ComputeHash(data);
            }
        }
    }
}
