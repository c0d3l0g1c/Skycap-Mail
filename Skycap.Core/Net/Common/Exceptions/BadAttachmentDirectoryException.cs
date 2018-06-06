namespace Skycap.Net.Common.Exceptions
{
    using System;

    public class BadAttachmentDirectoryException : ArgumentException
    {
        public BadAttachmentDirectoryException()
        {
        }

        public BadAttachmentDirectoryException(string message) : base(message)
        {
        }
    }
}

