namespace Skycap.Net.Common.Exceptions
{
    using System;

    public class AuthenticationMethodNotSupportedException : Exception
    {
        public AuthenticationMethodNotSupportedException()
        {
        }

        public AuthenticationMethodNotSupportedException(string message) : base(message)
        {
        }

        public AuthenticationMethodNotSupportedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

