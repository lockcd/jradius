using System;

namespace JRadius.Core.Exception
{
    public class InvalidParameterException : RadiusException
    {
        public InvalidParameterException() : this("An Invalid Parameter was sent to this method!")
        {
        }

        public InvalidParameterException(string message) : base(message)
        {
        }
    }
}
