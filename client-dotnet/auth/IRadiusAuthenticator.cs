using JRadius.Core.Packet;

namespace JRadius.Core.Client.Auth
{
    public interface IRadiusAuthenticator
    {
        string GetAuthName();
        void SetupRequest(RadiusClient client, RadiusPacket p);
        void ProcessRequest(RadiusPacket p);
        void ProcessChallenge(RadiusPacket request, RadiusPacket challenge);
    }
}
