namespace Skycap.Net.Pop3.Exceptions
{
    using System;

    public class Pop3ReceiveException : Pop3Exception
    {
        public Pop3ReceiveException()
        {
        }

        public Pop3ReceiveException(string message) : base(message)
        {
        }
    }
}

