using System;

namespace JRadius.Extended.Tls
{
    public class TlsRuntimeException : Exception
    {
        public TlsRuntimeException(string message)
            : base(message)
        {
        }

        public TlsRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
