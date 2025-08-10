using System;
using System.Security.Cryptography;
using System.Text;

namespace JRadius.Core.Util
{
    public static class MSCHAP
    {
        private static void ParityKey(byte[] szOut, byte[] szIn, int offset)
        {
            int cNext = 0;
            for (int i = 0; i < 7; i++)
            {
                int cWorking = 0xFF & szIn[i + offset];
                szOut[i] = (byte)(((cWorking >> i) | cNext | 1) & 0xff);
                cWorking = 0xFF & szIn[i + offset];
                cNext = (cWorking << (7 - i));
            }
            szOut[i] = (byte)(cNext | 1);
        }

        private static byte[] Unicode(byte[] input)
        {
            return Encoding.Unicode.GetBytes(Encoding.ASCII.GetString(input));
        }

        private static byte[] ChallengeHash(byte[] peerChallenge, byte[] authenticatorChallenge, byte[] userName)
        {
            var challenge = new byte[8];
            using (var sha1 = SHA1.Create())
            {
                var buffer = new byte[peerChallenge.Length + authenticatorChallenge.Length + userName.Length];
                Buffer.BlockCopy(peerChallenge, 0, buffer, 0, peerChallenge.Length);
                Buffer.BlockCopy(authenticatorChallenge, 0, buffer, peerChallenge.Length, authenticatorChallenge.Length);
                Buffer.BlockCopy(userName, 0, buffer, peerChallenge.Length + authenticatorChallenge.Length, userName.Length);
                var hash = sha1.ComputeHash(buffer);
                Buffer.BlockCopy(hash, 0, challenge, 0, 8);
            }
            return challenge;
        }

        private static byte[] NtPasswordHash(byte[] password)
        {
            using (var md4 = new MD4())
            {
                return md4.ComputeHash(Unicode(password));
            }
        }

        private static void DesEncrypt(byte[] clear, int clearOffset, byte[] key, int keyOffset, byte[] cypher, int cypherOffset)
        {
            var szParityKey = new byte[8];
            ParityKey(szParityKey, key, keyOffset);
            using (var des = DES.Create())
            {
                des.Key = szParityKey;
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.None;
                des.IV = new byte[8];
                using (var encryptor = des.CreateEncryptor())
                {
                    var output = encryptor.TransformFinalBlock(clear, clearOffset, clear.Length - clearOffset);
                    Buffer.BlockCopy(output, 0, cypher, cypherOffset, output.Length);
                }
            }
        }

        private static byte[] ChallengeResponse(byte[] challenge, byte[] passwordHash)
        {
            var response = new byte[24];
            var zPasswordHash = new byte[21];
            Buffer.BlockCopy(passwordHash, 0, zPasswordHash, 0, 16);
            DesEncrypt(challenge, 0, zPasswordHash, 0, response, 0);
            DesEncrypt(challenge, 0, zPasswordHash, 7, response, 8);
            DesEncrypt(challenge, 0, zPasswordHash, 14, response, 16);
            return response;
        }

        private static byte[] NtChallengeResponse(byte[] challenge, byte[] password)
        {
            var passwordHash = NtPasswordHash(password);
            return ChallengeResponse(challenge, passwordHash);
        }

        private static byte[] GenerateNTResponse(byte[] authenticatorChallenge, byte[] peerChallenge, byte[] userName, byte[] password)
        {
            var challenge = ChallengeHash(peerChallenge, authenticatorChallenge, userName);
            var passwordHash = NtPasswordHash(password);
            return ChallengeResponse(challenge, passwordHash);
        }

        public static byte[] DoMSCHAPv1(byte[] password, byte[] authChallenge)
        {
            var response = new byte[50];
            var ntResponse = NtChallengeResponse(authChallenge, password);
            Buffer.BlockCopy(ntResponse, 0, response, 26, 24);
            response[1] = 0x01;
            return response;
        }

        public static byte[] DoMSCHAPv2(byte[] userName, byte[] password, byte[] authChallenge)
        {
            var response = new byte[50];
            var peerChallenge = RadiusRandom.GetBytes(16);
            var ntResponse = GenerateNTResponse(authChallenge, peerChallenge, userName, password);
            Buffer.BlockCopy(peerChallenge, 0, response, 2, 16);
            Buffer.BlockCopy(ntResponse, 0, response, 26, 24);
            return response;
        }

        public static bool VerifyMSCHAPv2(byte[] userName, byte[] password, byte[] challenge, byte[] response)
        {
            var peerChallenge = new byte[16];
            var sentNtResponse = new byte[24];
            Buffer.BlockCopy(response, 2, peerChallenge, 0, 16);
            Buffer.BlockCopy(response, 26, sentNtResponse, 0, 24);
            var ntResponse = GenerateNTResponse(challenge, peerChallenge, userName, password);
            return ntResponse.AsSpan().SequenceEqual(sentNtResponse);
        }
    }
}
