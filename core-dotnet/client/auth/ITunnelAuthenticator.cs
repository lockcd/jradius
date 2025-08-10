using JRadius.Core.Packet.Attribute;

namespace JRadius.Core.Client.Auth
{
    public interface ITunnelAuthenticator
    {
        void SetTunneledAttributes(AttributeList attributes);
    }
}
