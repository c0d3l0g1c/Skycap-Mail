namespace Skycap.Net.Imap.Exceptions
{
    using System;

    public class UnexpectedResponseException : ImapException
    {
        public UnexpectedResponseException()
        {
        }

        public UnexpectedResponseException(string message) : base(message)
        {
        }
    }
}

