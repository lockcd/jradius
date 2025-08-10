namespace JRadius.Core.Packet
{
    public class PasswordReject : RadiusResponse
    {
        public const byte CODE = 9;

        public PasswordReject()
        {
            _code = CODE;
        }
    }
}
