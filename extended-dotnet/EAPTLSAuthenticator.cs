using JRadius.Core.Client.Auth;

namespace net.jradius.extended.client.auth
{
    public class EAPTLSAuthenticator : EAPAuthenticator
    {
        public override byte[] DoEAPType(byte id, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override string GetAuthName()
        {
            throw new NotImplementedException();
        }
    }
}
