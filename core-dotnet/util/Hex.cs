using System;
using System.Text;

namespace JRadius.Core.Util
{
    public class Hex
    {
        private static readonly string[] pseudo = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new System.Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static string ByteArrayToHexString(byte[] inBytes)
        {
            if (inBytes == null || inBytes.Length <= 0)
                return null;

            StringBuilder sb = new StringBuilder(inBytes.Length * 2);

            foreach (byte b in inBytes)
            {
                sb.Append(pseudo[(b & 0xF0) >> 4]);
                sb.Append(pseudo[b & 0x0F]);
            }

            return sb.ToString();
        }
    }
}
