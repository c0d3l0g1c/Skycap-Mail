namespace Skycap.Net.Pop3.Exceptions
{
    using System;

    public class Pop3Exception : Exception
    {
        public Pop3Exception()
        {
        }

        public Pop3Exception(string message) : base(message)
        {
        }
    }
}

