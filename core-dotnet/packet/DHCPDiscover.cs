namespace JRadius.Core.Packet
{
    public class DHCPDiscover : DHCPPacket
    {
        public const int CODE = 1024 + 1;

        public DHCPDiscover()
        {
            _code = CODE;
        }
    }
}
