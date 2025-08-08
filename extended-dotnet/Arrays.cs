namespace JRadius.Extended.Tls
{
    public class Arrays
    {
        public static bool ConstantTimeAreEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            int i = a.Length;
            int j = 0;
            while (i-- > 0)
            {
                j |= a[i] ^ b[i];
            }
            return j == 0;
        }
    }
}
