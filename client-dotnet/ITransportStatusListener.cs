using JRadius.Core.Packet;

namespace JRadius.Core.Client
{
    public interface ITransportStatusListener
    {
        void OnBeforeReceive(RadiusClientTransport transport);
        void OnAfterReceive(RadiusClientTransport transport, RadiusPacket packet);
        void OnBeforeSend(RadiusClientTransport transport, RadiusPacket packet);
        void OnAfterSend(RadiusClientTransport transport);
    }
}
