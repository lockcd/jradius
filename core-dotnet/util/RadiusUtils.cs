using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JRadius.Core.Util
{
    public static class RadiusUtils
    {
        public static byte[] EncodePapPassword(byte[] userPass, byte[] requestAuthenticator, string sharedSecret)
        {
            byte[] userPassBytes;
            if (userPass.Length > 128)
            {
                userPassBytes = new byte[128];
                System.Array.Copy(userPass, 0, userPassBytes, 0, 128);
            }
            else
            {
                userPassBytes = userPass;
            }

            byte[] encryptedPass;
            if (userPassBytes.Length < 128)
            {
                if (userPassBytes.Length % 16 == 0)
                {
                    encryptedPass = new byte[userPassBytes.Length];
                }
                else
                {
                    encryptedPass = new byte[((userPassBytes.Length / 16) * 16) + 16];
                }
            }
            else
            {
                encryptedPass = new byte[128];
            }

            System.Array.Copy(userPassBytes, 0, encryptedPass, 0, userPassBytes.Length);
            for (int i = userPassBytes.Length; i < encryptedPass.Length; i++)
            {
                encryptedPass[i] = 0;
            }

            using (var md5 = MD5.Create())
            {
                var sharedSecretBytes = Encoding.UTF8.GetBytes(sharedSecret);
                var bn = md5.ComputeHash(sharedSecretBytes.Concat(requestAuthenticator).ToArray());

                for (int i = 0; i < 16; i++)
                {
                    encryptedPass[i] = (byte)(bn[i] ^ encryptedPass[i]);
                }

                if (encryptedPass.Length > 16)
                {
                    for (int i = 16; i < encryptedPass.Length; i += 16)
                    {
                        var prevEncrypted = new byte[16];
                        System.Array.Copy(encryptedPass, i - 16, prevEncrypted, 0, 16);
                        bn = md5.ComputeHash(sharedSecretBytes.Concat(prevEncrypted).ToArray());
                        for (int j = 0; j < 16; j++)
                        {
                            encryptedPass[i + j] = (byte)(bn[j] ^ encryptedPass[i + j]);
                        }
                    }
                }
            }

            return encryptedPass;
        }

        public static byte[] MakeRFC2865RequestAuthenticator(string sharedSecret)
        {
            using (var md5 = MD5.Create())
            {
                var requestAuthenticator = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(requestAuthenticator);
                }
                var sharedSecretBytes = Encoding.UTF8.GetBytes(sharedSecret);
                return md5.ComputeHash(sharedSecretBytes.Concat(requestAuthenticator).ToArray());
            }
        }

        public static byte[] MakeRFC2865ResponseAuthenticator(string sharedSecret, byte code, byte identifier, short length, byte[] requestAuthenticator, byte[] responseAttributeBytes, int responseAttributeLength)
        {
            using (var md5 = MD5.Create())
            {
                var buffer = new byte[4 + requestAuthenticator.Length + responseAttributeLength + sharedSecret.Length];
                buffer[0] = code;
                buffer[1] = identifier;
                buffer[2] = (byte)(length >> 8);
                buffer[3] = (byte)(length & 0xff);
                Buffer.BlockCopy(requestAuthenticator, 0, buffer, 4, requestAuthenticator.Length);
                Buffer.BlockCopy(responseAttributeBytes, 0, buffer, 4 + requestAuthenticator.Length, responseAttributeLength);
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(sharedSecret), 0, buffer, 4 + requestAuthenticator.Length + responseAttributeLength, sharedSecret.Length);
                return md5.ComputeHash(buffer);
            }
        }
    }
}
