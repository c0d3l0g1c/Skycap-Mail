namespace Skycap.Net.Imap.Exceptions
{
    using System;

    public class ImapException : Exception
    {
        public ImapException()
        {
        }

        public ImapException(string message) : base(message)
        {
        }
    }
}

