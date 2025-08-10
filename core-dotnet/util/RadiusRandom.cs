using System;
using System.Text;

namespace JRadius.Core.Util
{
    public static class RadiusRandom
    {
        private static readonly Random _rand = new Random();

        public static byte[] GetBytes(int length)
        {
            var result = new byte[length];
            lock (_rand)
            {
                _rand.NextBytes(result);
            }
            return result;
        }

        public static string GetRandomPassword(int length)
        {
            string[] pseudo = { "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "o", "p", "q", "r", "u", "s", "t", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            var out_Renamed = new StringBuilder(length);
            byte[] in_Renamed = GetBytes(length);
            for (int i = 0; i < length; i++)
            {
                out_Renamed.Append(pseudo[in_Renamed[i] % pseudo.Length]);
            }
            return out_Renamed.ToString();
        }

        public static string GetRandomPassword(int length, string allowedCharacters)
        {
            var out_Renamed = new StringBuilder(length);
            byte[] in_Renamed = GetBytes(length);
            for (int i = 0; i < length; i++)
            {
                out_Renamed.Append(allowedCharacters[in_Renamed[i] % allowedCharacters.Length]);
            }
            return out_Renamed.ToString();
        }

        public static string GetRandomString(int length)
        {
            return Hex.ByteArrayToHexString(GetBytes(length));
        }
    }
}
