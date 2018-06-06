namespace Skycap.Net.Imap.Exceptions
{
    using System;

    public class ExpectedResponseException : ImapException
    {
        public ExpectedResponseException()
        {
        }

        public ExpectedResponseException(string message) : base(message)
        {
        }
    }
}

