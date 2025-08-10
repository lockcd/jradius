namespace JRadius.Core.Packet
{
    public class DisconnectACK : DisconnectResponse
    {
        public const byte CODE = 41;

        public DisconnectACK()
        {
            _code = CODE;
        }
    }
}
