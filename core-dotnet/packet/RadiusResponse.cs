using JRadius.Core.Packet.Attribute;
using JRadius.Core.Util;
using System.IO;
using System.Linq;

namespace JRadius.Core.Packet
{
    public abstract class RadiusResponse : RadiusPacket
    {
        public RadiusResponse()
            : base()
        {
        }

        public RadiusResponse(int id, AttributeList list)
            : base(list)
        {
            SetIdentifier(id);
        }

        public bool VerifyAuthenticator(byte[] requestAuthenticator, string sharedSecret)
        {
            var buffer = new MemoryStream(4096);
            // TODO: RadiusFormat.GetInstance().PackAttributeList(GetAttributes(), buffer, true);
            var hash = RadiusUtils.MakeRFC2865ResponseAuthenticator(sharedSecret,
                (byte)(GetCode() & 0xff), (byte)(GetIdentifier() & 0xff),
                (short)(buffer.Position + RADIUS_HEADER_LENGTH),
                requestAuthenticator, buffer.GetBuffer(), (int)buffer.Position);
            return hash.SequenceEqual(GetAuthenticator());
        }

        public void GenerateAuthenticator(byte[] requestAuthenticator, string sharedSecret)
        {
            var buffer = new MemoryStream(4096);
            // TODO: RadiusFormat.GetInstance().PackAttributeList(GetAttributes(), buffer, true);
            SetAuthenticator(RadiusUtils.MakeRFC2865ResponseAuthenticator(sharedSecret,
                (byte)(GetCode() & 0xff), (byte)(GetIdentifier() & 0xff),
                (short)(buffer.Position + RADIUS_HEADER_LENGTH),
                requestAuthenticator, buffer.GetBuffer(), (int)buffer.Position));
        }
    }
}
