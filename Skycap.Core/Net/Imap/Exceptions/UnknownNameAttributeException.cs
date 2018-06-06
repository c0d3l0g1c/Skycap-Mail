namespace Skycap.Net.Imap.Exceptions
{
    using System;

    public class UnknownNameAttributeException : ImapException
    {
        public UnknownNameAttributeException()
        {
        }

        public UnknownNameAttributeException(string message) : base(message)
        {
        }
    }
}

