namespace Skycap.Net.Pop3.Exceptions
{
    using System;

    internal class Pop3WrongStateException : Pop3Exception
    {
        public Pop3WrongStateException()
        {
        }

        public Pop3WrongStateException(string message) : base(message)
        {
        }
    }
}

