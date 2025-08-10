using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace JRadius.Extended.Util
{
    public static class KeyStoreUtil
    {
        public static void LoadBC()
        {
            // Bouncy Castle is already loaded in C#
        }

        public static X509Certificate2Collection LoadKeyManager(string type, Stream in_Renamed, string password)
        {
            LoadBC();
            var collection = new X509Certificate2Collection();
            if (type.Equals("pem", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Implement PEM loading
            }
            else
            {
                collection.Import(ReadFully(in_Renamed), password, X509KeyStorageFlags.DefaultKeySet);
            }
            return collection;
        }

        public static X509Certificate2Collection LoadTrustManager(string type, Stream in_Renamed, string password)
        {
            LoadBC();
            var collection = new X509Certificate2Collection();
            if (type.Equals("pem", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Implement PEM loading
            }
            else
            {
                collection.Import(ReadFully(in_Renamed), password, X509KeyStorageFlags.DefaultKeySet);
            }
            return collection;
        }

        public static bool TrustAllManager(object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
