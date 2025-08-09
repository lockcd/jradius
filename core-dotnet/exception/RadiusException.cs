using System;

namespace JRadius.Core.Exception
{
    public class RadiusException : System.Exception
    {
        public RadiusException()
        {
        }

        public RadiusException(string message) : base(message)
        {
        }

        public RadiusException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
