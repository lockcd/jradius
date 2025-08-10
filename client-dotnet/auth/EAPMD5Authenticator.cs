using JRadius.Core.Util;

namespace JRadius.Core.Client.Auth
{
    public class EAPMD5Authenticator : EAPAuthenticator
    {
        public const string NAME = "eap-md5";

        public EAPMD5Authenticator()
        {
            SetEAPType(EAP_MD5);
        }

        public override string GetAuthName()
        {
            return NAME;
        }

        public override byte[] DoEAPType(byte id, byte[] data)
        {
            byte md5len = data[0];
            var md5data = new byte[md5len];
            System.Array.Copy(data, 1, md5data, 0, md5len);
            var response = new byte[17];
            response[0] = 16;
            System.Array.Copy(CHAP.ChapMD5(id, GetPassword(), md5data), 0, response, 1, 16);
            return response;
        }
    }
}
