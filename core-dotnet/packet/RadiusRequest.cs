using JRadius.Core.Client;
using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Packet
{
    public abstract class RadiusRequest : RadiusPacket
    {
        protected RadiusClient _client = null;

        public RadiusRequest()
        {
        }

        public RadiusRequest(RadiusClient client)
        {
            _client = client;
        }

        public RadiusRequest(AttributeList attributes)
            : base(attributes)
        {
        }

        public RadiusRequest(RadiusClient client, AttributeList attributes)
            : base(attributes)
        {
            _client = client;
        }

        public void SetRadiusClient(RadiusClient client)
        {
            _client = client;
        }
    }
}
