namespace Skycap.Net.Pop3.Exceptions
{
    using System;

    public class Pop3SendException : Pop3Exception
    {
        public Pop3SendException()
        {
        }

        public Pop3SendException(string message) : base(message)
        {
        }
    }
}

