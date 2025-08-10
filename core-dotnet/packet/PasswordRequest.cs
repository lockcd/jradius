namespace JRadius.Core.Packet
{
    public class PasswordRequest : AccessRequest
    {
        public const byte CODE = 7;

        public PasswordRequest()
        {
            _code = CODE;
        }
    }
}
