namespace JRadius.Core.Packet
{
    public class DisconnectNAK : DisconnectResponse
    {
        public const byte CODE = 42;

        public DisconnectNAK()
        {
            _code = CODE;
        }
    }
}
