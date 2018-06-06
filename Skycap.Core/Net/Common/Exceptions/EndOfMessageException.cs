namespace Skycap.Net.Common.Exceptions
{
    using System;

    public class EndOfMessageException : Exception
    {
        public EndOfMessageException()
        {
        }

        public EndOfMessageException(string message) : base(message)
        {
        }

        public EndOfMessageException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

