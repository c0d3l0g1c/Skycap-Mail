namespace Skycap.Net.Imap.Exceptions
{
    using System;

    public class IncorrectResponseException : ImapException
    {
        public IncorrectResponseException()
        {
        }

        public IncorrectResponseException(string message) : base(message)
        {
        }
    }
}

