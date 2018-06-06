namespace Skycap.Net.Imap.Exceptions
{
    using System;

    public class BadMailboxException : Exception
    {
        public BadMailboxException(string message) : base(message)
        {
        }
    }
}

