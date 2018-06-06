namespace Skycap.Net.Pop3.Exceptions
{
    using System;

    public class Pop3ConnectException : Pop3Exception
    {
        public Pop3ConnectException()
        {
        }

        public Pop3ConnectException(string message) : base(message)
        {
        }
    }
}

