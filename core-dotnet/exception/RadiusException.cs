using System;

namespace net.jradius.core.exception
{
    public class RadiusException : Exception
    {
        public RadiusException(string message) : base(message)
        {
        }
    }
}
