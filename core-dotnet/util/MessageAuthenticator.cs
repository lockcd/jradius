using JRadius.Core.Packet;
using JRadius.Core.Packet.Attribute;
using System.IO;
using System.Linq;
using System.Text;

namespace JRadius.Core.Util
{
    public static class MessageAuthenticator
    {
        private static readonly RadiusFormat _format = RadiusFormat.GetInstance();

        public static void GenerateRequestMessageAuthenticator(RadiusPacket request, string sharedSecret)
        {
            var hash = new byte[16];
            var buffer = new MemoryStream(4096);
            request.OverwriteAttribute(new Attr_Message_Authenticator(hash));
            _format.PackPacket(request, sharedSecret, buffer, true);
            var key = Encoding.UTF8.GetBytes(sharedSecret);
            var computedHash = MD5.HmacMd5(buffer.ToArray(), 0, (int)buffer.Position, key);
            System.Array.Copy(computedHash, 0, hash, 0, 16);
        }

        public static bool VerifyReply(byte[] requestAuth, RadiusResponse reply, string sharedSecret)
        {
            var replyAuth = reply.GetAuthenticator();
            var hash = new byte[16];
            var buffer = new MemoryStream(4096);
            var attr = reply.FindAttribute(80); // Message-Authenticator
            if (attr == null)
            {
                return true; // Or false if required? The original code returns null.
            }

            var pval = attr.GetValue().GetBytes();
            attr.GetValue().SetValue(hash);
            reply.SetAuthenticator(requestAuth);
            _format.PackPacket(reply, sharedSecret, buffer, true);
            var key = Encoding.UTF8.GetBytes(sharedSecret);
            var computedHash = MD5.HmacMd5(buffer.ToArray(), 0, (int)buffer.Position, key);
            System.Array.Copy(computedHash, 0, hash, 0, 16);
            reply.SetAuthenticator(replyAuth);
            return pval.SequenceEqual(hash);
        }
    }
}
