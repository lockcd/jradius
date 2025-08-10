using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using JRadius.Core.Util;
using JRadius.Dictionary;
using System;

namespace JRadius.Core.Client.Auth
{
    public class MSCHAPv1Authenticator : RadiusAuthenticator
    {
        public const string NAME = "mschapv1";

        public override string GetAuthName()
        {
            return NAME;
        }

        public override void ProcessRequest(RadiusPacket p)
        {
            if (_password == null)
            {
                throw new System.Exception("no password given");
            }

            p.RemoveAttribute(_password);

            var authChallenge = RadiusRandom.GetBytes(16);
            var chapResponse = MSCHAP.DoMSCHAPv1(_password.GetValue().GetBytes(), authChallenge);

            var chapChallengeAttr = new Attr_MSCHAPChallenge();
            chapChallengeAttr.SetValue(authChallenge);
            p.AddAttribute(chapChallengeAttr);

            var chapResponseAttr = new Attr_MSCHAPResponse();
            chapResponseAttr.SetValue(chapResponse);
            p.AddAttribute(chapResponseAttr);
        }
    }
}
