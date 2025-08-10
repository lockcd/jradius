using System;
using System.Text;

namespace JRadius.Core.Util
{
    public static class Hex
    {
        private static readonly string[] pseudo = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

        public static byte[] HexStringToByteArray(string hex)
        {
            int len = hex.Length;
            byte[] bin = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                bin[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bin;
        }

        public static string ByteArrayToHexString(byte[] in_Renamed)
        {
            if (in_Renamed == null || in_Renamed.Length <= 0)
                return null;

            var sb = new StringBuilder(in_Renamed.Length * 2);
            foreach (byte b in in_Renamed)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
