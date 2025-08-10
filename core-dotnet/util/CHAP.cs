using System.Linq;
using System.Security.Cryptography;

namespace JRadius.Core.Util
{
    public static class CHAP
    {
        public static byte[] ChapMD5(byte id, byte[] password, byte[] challenge)
        {
            using (var md5 = MD5.Create())
            {
                var idBytes = new byte[] { id };
                return md5.ComputeHash(idBytes.Concat(password).Concat(challenge).ToArray());
            }
        }

        public static byte[] ChapResponse(byte id, byte[] password, byte[] challenge)
        {
            var response = new byte[17];
            response[0] = id;
            System.Array.Copy(ChapMD5(id, password, challenge), 0, response, 1, 16);
            return response;
        }
    }
}
