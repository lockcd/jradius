namespace JRadius.Core.Util
{
    public class CHAP
    {
        public static byte[] ChapMD5(byte id, byte[] password, byte[] challenge)
        {
            if (password == null || challenge == null)
            {
                return null;
            }

            // The Java version updates the digest with each part separately.
            // Concatenating them into a single array before hashing is equivalent
            // and simpler to write in C#.
            byte[] combined = new byte[1 + password.Length + challenge.Length];
            combined[0] = id;
            System.Buffer.BlockCopy(password, 0, combined, 1, password.Length);
            System.Buffer.BlockCopy(challenge, 0, combined, 1 + password.Length, challenge.Length);

            return MD5.GetHash(combined);
        }

        public static byte[] ChapResponse(byte id, byte[] password, byte[] challenge)
        {
            byte[] hash = ChapMD5(id, password, challenge);
            if (hash == null)
            {
                return null;
            }

            byte[] response = new byte[1 + hash.Length];
            response[0] = id;
            System.Buffer.BlockCopy(hash, 0, response, 1, hash.Length);
            return response;
        }
    }
}
