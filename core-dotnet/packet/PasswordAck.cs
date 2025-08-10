namespace JRadius.Core.Packet
{
    public class PasswordAck : RadiusResponse
    {
        public const byte CODE = 8;

        public PasswordAck()
        {
            _code = CODE;
        }
    }
}
