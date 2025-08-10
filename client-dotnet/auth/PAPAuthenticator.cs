using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using JRadius.Core.Util;
using System;
using System.Linq;

namespace JRadius.Core.Client.Auth
{
    public class PAPAuthenticator : RadiusAuthenticator
    {
        public const string NAME = "pap";

        public override string GetAuthName()
        {
            return NAME;
        }

        public override void ProcessRequest(RadiusPacket p)
        {
            if (_password == null)
            {
                throw new Exception("no password given");
            }

            p.RemoveAttribute(_password);

            var attr = new Attr_UserPassword();
            attr.SetValue(RadiusUtils.EncodePapPassword(
                _password.GetValue().GetBytes(),
                p.CreateAuthenticator(null, 0, 0, _client.GetSharedSecret()),
                _client.GetSharedSecret()));
            p.AddAttribute(attr);
        }

        public static bool VerifyPassword(byte[] userPassword, byte[] requestAuthenticator, byte[] clearText, string sharedSecret)
        {
            byte[] pw = RadiusUtils.EncodePapPassword(clearText, requestAuthenticator, sharedSecret);
            return userPassword.SequenceEqual(pw);
        }
    }
}
