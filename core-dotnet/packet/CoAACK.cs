namespace JRadius.Core.Packet
{
    public class CoAACK : CoAResponse
    {
        public const byte CODE = 44;

        public CoAACK()
        {
            _code = CODE;
        }
    }
}
