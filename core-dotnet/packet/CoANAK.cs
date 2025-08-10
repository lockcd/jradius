namespace JRadius.Core.Packet
{
    public class CoANAK : CoAResponse
    {
        public const byte CODE = 45;

        public CoANAK()
        {
            _code = CODE;
        }
    }
}
