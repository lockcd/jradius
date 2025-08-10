using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using JRadius.Core.Util;
using System;
using System.Linq;

namespace JRadius.Core.Client.Auth
{
    public class CHAPAuthenticator : RadiusAuthenticator
    {
        public const string NAME = "chap";

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

            var authChallenge = RadiusRandom.GetBytes(16);
            var chapResponse = CHAP.ChapResponse((byte)p.GetIdentifier(), _password.GetValue().GetBytes(), authChallenge);

            var chapChallengeAttr = new Attr_CHAPChallenge();
            chapChallengeAttr.SetValue(authChallenge);
            p.AddAttribute(chapChallengeAttr);

            var chapPasswordAttr = new Attr_CHAPPassword();
            chapPasswordAttr.SetValue(chapResponse);
            p.AddAttribute(chapPasswordAttr);
        }

        public static bool VerifyPassword(byte[] response, byte[] challenge, byte id, byte[] clearText)
        {
            byte[] chapResponse = CHAP.ChapResponse(response[0], clearText, challenge);
            return response.SequenceEqual(chapResponse);
        }
    }
}
