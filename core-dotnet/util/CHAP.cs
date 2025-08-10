using System.Linq;


namespace JRadius.Core.Util
{
    public static class CHAP
    {
        public static byte[] ChapMD5(byte id, byte[] password, byte[] challenge)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var idBytes = new byte[] { id };
                return md5.ComputeHash(idBytes.Concat(password).Concat(challenge).ToArray());
            }

            //// The Java version updates the digest with each part separately.
            //// Concatenating them into a single array before hashing is equivalent
            //// and simpler to write in C#.
            //byte[] combined = new byte[1 + password.Length + challenge.Length];
            //combined[0] = id;
            //System.Buffer.BlockCopy(password, 0, combined, 1, password.Length);
            //System.Buffer.BlockCopy(challenge, 0, combined, 1 + password.Length, challenge.Length);

            //return MD5.GetHash(combined);
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
