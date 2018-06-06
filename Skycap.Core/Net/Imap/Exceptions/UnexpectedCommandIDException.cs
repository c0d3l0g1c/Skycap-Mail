namespace Skycap.Net.Imap.Exceptions
{
    using System;

    public class UnexpectedCommandIDException : ImapException
    {
        public UnexpectedCommandIDException()
        {
        }

        public UnexpectedCommandIDException(string message) : base(message)
        {
        }
    }
}

