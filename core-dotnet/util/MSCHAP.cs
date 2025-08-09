using System;
using System.Security.Cryptography;
using System.Text;

namespace JRadius.Core.Util
{
    public class MSCHAP
    {
        // Note: This class is a port of the MSCHAP.java file.
        // The logic must be ported carefully to ensure compatibility.

        /// <summary>
        /// Sets the parity bits on a 7-byte key to create an 8-byte DES key.
        /// </summary>
        private static void SetParityKey(byte[] key7, int offset, byte[] key8)
        {
            int cNext = 0;
            for (int i = 0; i < 7; i++)
            {
                int cWorking = key7[i + offset] & 0xFF;
                key8[i] = (byte)(((cWorking >> i) | cNext | 1) & 0xFF);
                cWorking = key7[i + offset] & 0xFF;
                cNext = (cWorking << (7 - i));
            }
            key8[7] = (byte)((cNext | 1) & 0xFF);
        }

        /// <summary>
        /// Encrypts an 8-byte block with a 7-byte key using DES.
        /// </summary>
        private static void DesEncrypt(byte[] clear, byte[] key7, byte[] cipher)
        {
            byte[] key8 = new byte[8];
            SetParityKey(key7, 0, key8);

            using (var des = DES.Create())
            {
                des.Key = key8;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.None;
                des.IV = new byte[8]; // Zero IV

                using (var encryptor = des.CreateEncryptor())
                {
                    // The Java code seems to encrypt the whole block.
                    // .NET's TransformBlock can be tricky. Using DoFinal equivalent.
                    byte[] encrypted = encryptor.TransformFinalBlock(clear, 0, clear.Length);
                    Array.Copy(encrypted, 0, cipher, 0, encrypted.Length);
                }
            }
        }
        
        /// <summary>
        /// Computes the NT Password Hash, which is the MD4 hash of the password.
        /// </summary>
        private static byte[] NtPasswordHash(string password)
        {
            // MD4 is obsolete and not included in .NET's standard libraries.
            // A custom implementation is required to match the Java code.
            // This is a critical missing piece.
            // For now, returning a dummy value.
            Console.Error.WriteLine("CRITICAL: MD4 hash algorithm is not implemented.");
            return new byte[16]; 
        }

        /// <summary>
        /// Calculates the NT Challenge-Response for MS-CHAP.
        /// </summary>
        public static byte[] NtChallengeResponse(byte[] challenge, string password)
        {
            byte[] passwordHash = NtPasswordHash(password);
            
            // The full password hash is 16 bytes. A 21-byte version with 5 zero-bytes
            // appended is used for the challenge-response.
            byte[] zPasswordHash = new byte[21];
            Array.Copy(passwordHash, 0, zPasswordHash, 0, 16);

            byte[] response = new byte[24];
            
            // Encrypt the 8-byte challenge with three different 7-byte keys derived from the hash.
            byte[] key1 = new byte[7];
            Array.Copy(zPasswordHash, 0, key1, 0, 7);
            DesEncrypt(challenge, key1, response); // First 8 bytes of response

            byte[] key2 = new byte[7];
            Array.Copy(zPasswordHash, 7, key2, 0, 7);
            byte[] tempResponse2 = new byte[8];
            DesEncrypt(challenge, key2, tempResponse2);
            Array.Copy(tempResponse2, 0, response, 8, 8); // Next 8 bytes

            byte[] key3 = new byte[7];
            Array.Copy(zPasswordHash, 14, key3, 0, 7);
            byte[] tempResponse3 = new byte[8];
            DesEncrypt(challenge, key3, tempResponse3);
            Array.Copy(tempResponse3, 0, response, 16, 8); // Final 8 bytes
            
            return response;
        }
    }
}
