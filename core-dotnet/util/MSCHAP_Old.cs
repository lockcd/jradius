using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace JRadius.Core.Util
{
    /// <summary>
    /// An MSCHAP implementation translated into C# from the original
    /// pseudocode found in RFC 2759 and 2433.
    /// </summary>
    public sealed partial class MSCHAP_Old
    {
        /// <summary>
        /// Adjusts the key parity to conform to DES requirements.
        /// </summary>
        private static void SetParityKey(byte[] output, byte[] input, int offset)
        {
            for (int i = 0; i < 7; i++)
            {
                byte cWorking = input[i + offset];
                // Set the parity bit for each byte.
                // The parity bit is the least significant bit.
                // This algorithm ensures an odd number of set bits.
                byte cNext = (byte)(cWorking << (7 - i));
                output[i] = (byte)(((cWorking >> i) | cNext | 1) & 0xFF);
            }
            output[7] = (byte)(((input[6 + offset] << 1) | 1) & 0xFF);
        }

        /// <summary>
        /// Converts an ASCII byte array to a Unicode (UTF-16LE) byte array.
        /// </summary>
        private static byte[] ToUnicode(byte[] input)
        {
            // Encoding.Unicode in .NET corresponds to UTF-16LE, which is what's needed.
            return Encoding.Unicode.GetBytes(Encoding.ASCII.GetString(input));
        }

        /// <summary>
        /// Creates the 8-byte challenge hash as specified in RFC 2759, Section 3.
        /// Hash = SHA1(Peer-Challenge + Authenticator-Challenge + User-Name)[0..7]
        /// </summary>
        private static byte[] CreateChallengeHash(byte[] peerChallenge, byte[] authenticatorChallenge, byte[] userName)
        {
            var challenge = new byte[8];
            using (var sha1 = SHA1.Create())
            {
                // Combine the challenges and username before hashing.
                sha1.TransformBlock(peerChallenge, 0, 16, null, 0);
                sha1.TransformBlock(authenticatorChallenge, 0, 16, null, 0);
                sha1.TransformFinalBlock(userName, 0, userName.Length);

                byte[] hash = sha1.Hash;
                // Use the first 8 bytes of the SHA1 hash.
                Array.Copy(hash, 0, challenge, 0, 8);
            }
            return challenge;
        }

        /// <summary>
        /// Creates the 16-byte NT Password Hash.
        /// Hash = MD4(Unicode(Password))
        /// </summary>
        private static byte[] CreateNtPasswordHash(byte[] password)
        {
            byte[] unicodePassword = ToUnicode(password);
            using (var md4 = new MD4())
            {
                return md4.ComputeHash(unicodePassword);
            }
        }

        /// <summary>
        /// Encrypts an 8-byte cleartext block using a 7-byte key with DES.
        /// </summary>
        private static void EncryptDes(byte[] clear, int clearOffset, byte[] key, int keyOffset, byte[] cypher, int cypherOffset)
        {
            var parityKey = new byte[8];
            SetParityKey(parityKey, key, keyOffset);

            try
            {
                using (var des = DES.Create())
                {
                    des.Key = parityKey;
                    des.Mode = CipherMode.CBC;
                    des.Padding = PaddingMode.None;
                    des.IV = new byte[8]; // Zero IV

                    using (var encryptor = des.CreateEncryptor())
                    {
                        // MS-CHAP DES encryption always operates on 8-byte blocks.
                        byte[] encrypted = encryptor.TransformFinalBlock(clear, clearOffset, 8);
                        Array.Copy(encrypted, 0, cypher, cypherOffset, encrypted.Length);
                    }
                }
            }
            catch (System.Exception e)
            {
                // Log the exception instead of letting it bubble up.
                Trace.TraceWarning($"DES Encryption failed: {e.Message}");
            }
        }

        /// <summary>
        /// Generates a 24-byte response by encrypting an 8-byte challenge with a 16-byte password hash.
        /// The password hash is split into three 7-byte keys to perform three DES encryptions.
        /// </summary>
        private static byte[] CreateChallengeResponse(byte[] challenge, byte[] passwordHash)
        {
            var response = new byte[24];
            var zPasswordHash = new byte[21];

            // Copy the 16-byte hash and pad with 5 zero bytes.
            Array.Copy(passwordHash, 0, zPasswordHash, 0, 16);

            // Encrypt the challenge three times with different parts of the key.
            EncryptDes(challenge, 0, zPasswordHash, 0, response, 0);  // Key from bytes 0-6
            EncryptDes(challenge, 0, zPasswordHash, 7, response, 8);  // Key from bytes 7-13
            EncryptDes(challenge, 0, zPasswordHash, 14, response, 16); // Key from bytes 14-20

            return response;
        }

        /// <summary>
        /// Generates the NT Challenge Response.
        /// </summary>
        private static byte[] CreateNtChallengeResponse(byte[] challenge, byte[] password)
        {
            byte[] passwordHash = CreateNtPasswordHash(password);
            return CreateChallengeResponse(challenge, passwordHash);
        }

        /// <summary>
        /// Generates the complete NT-Response for MS-CHAPv2.
        /// </summary>
        private static byte[] GenerateNTResponse(byte[] authenticatorChallenge, byte[] peerChallenge, byte[] userName, byte[] password)
        {
            byte[] challenge = CreateChallengeHash(peerChallenge, authenticatorChallenge, userName);
            byte[] passwordHash = CreateNtPasswordHash(password);
            return CreateChallengeResponse(challenge, passwordHash);
        }

        /// <summary>
        /// Creates a one-way hash of a key using a standard text "KGS!@#$%" as the data.
        /// This is used for the LM hash.
        /// </summary>
        public static void CreateDesHash(byte[] key, int offsetKey, byte[] cypher, int offsetCypher)
        {
            byte[] clearText = Encoding.ASCII.GetBytes("KGS!@#$%");
            EncryptDes(clearText, 0, key, offsetKey, cypher, offsetCypher);
        }

        /// <summary>
        /// Creates the 16-byte LanManager (LM) Password Hash.
        /// </summary>
        public static byte[] CreateLmPasswordHash(byte[] password)
        {
            var passwordHash = new byte[16];
            var pByte = new byte[14]; // Automatically initialized to zeros.

            // Convert password to uppercase and copy up to 14 bytes.
            string pString = Encoding.ASCII.GetString(password).ToUpperInvariant();
            byte[] passwordBytes = Encoding.ASCII.GetBytes(pString);
            Array.Copy(passwordBytes, 0, pByte, 0, Math.Min(14, passwordBytes.Length));

            // Create two 8-byte hashes from the 14-byte password.
            CreateDesHash(pByte, 0, passwordHash, 0);
            CreateDesHash(pByte, 7, passwordHash, 8);

            return passwordHash;
        }

        /// <summary>
        /// Generates the LM Challenge Response.
        /// </summary>
        public static byte[] CreateLmChallengeResponse(byte[] challenge, byte[] password)
        {
            byte[] passwordHash = CreateLmPasswordHash(password);
            return CreateChallengeResponse(challenge, passwordHash);
        }

        /// <summary>
        /// Performs MS-CHAPv1 authentication.
        /// </summary>
        /// <param name="password">The user's password.</param>
        /// <param name="authChallenge">The 16-byte authenticator challenge.</param>
        /// <returns>A 50-byte array containing the MS-CHAP response.</returns>
        public static byte[] DoMSCHAPv1(byte[] password, byte[] authChallenge)
        {
            var response = new byte[50];
            byte[] lmResponse = CreateLmChallengeResponse(authChallenge, password);
            byte[] ntResponse = CreateNtChallengeResponse(authChallenge, password);

            // The response format is:
            // 1 byte: LmResponseUsed (0 or 1)
            // 1 byte: NtResponseUsed (0 or 1)
            // 24 bytes: LmChallengeResponse
            // 24 bytes: NtChallengeResponse
            Array.Copy(lmResponse, 0, response, 2, 24);
            Array.Copy(ntResponse, 0, response, 26, 24);
            response[1] = 0x01; // Indicate that NT Response is used.
            return response;
        }

        /// <summary>
        /// Performs MS-CHAPv2 authentication.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="authChallenge">The 16-byte authenticator challenge.</param>
        /// <returns>A 50-byte array containing the MS-CHAPv2 response.</returns>
        public static byte[] DoMSCHAPv2(byte[] userName, byte[] password, byte[] authChallenge)
        {
            var response = new byte[50];
            var peerChallenge = new byte[16];

            // Generate a 16-byte cryptographically secure random peer challenge.
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(peerChallenge);
            }

            byte[] ntResponse = GenerateNTResponse(authChallenge, peerChallenge, userName, password);

            // The response format is:
            // 1 byte: Identifier
            // 1 byte: Flags
            // 16 bytes: Peer-Challenge
            // 8 bytes: Reserved (zeros)
            // 24 bytes: NT-Response
            Array.Copy(peerChallenge, 0, response, 2, 16);
            Array.Copy(ntResponse, 0, response, 26, 24);

            return response;
        }

        /// <summary>
        /// Verifies an MS-CHAPv2 response from a client.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="challenge">The original 16-byte authenticator challenge sent to the client.</param>
        /// <param name="response">The 50-byte response received from the client.</param>
        /// <returns>True if the response is valid, otherwise false.</returns>
        public static bool VerifyMSCHAPv2(byte[] userName, byte[] password, byte[] challenge, byte[] response)
        {
            var peerChallenge = new byte[16];
            var sentNtResponse = new byte[24];

            // Extract the Peer-Challenge and NT-Response from the client's response.
            Array.Copy(response, 2, peerChallenge, 0, 16);
            Array.Copy(response, 26, sentNtResponse, 0, 24);

            // Generate the expected response on the server side.
            byte[] expectedNtResponse = GenerateNTResponse(challenge, peerChallenge, userName, password);

            // Compare the expected response with the one sent by the client.
            return expectedNtResponse.SequenceEqual(sentNtResponse);
        }

    }

}
