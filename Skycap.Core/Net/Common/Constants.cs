namespace Skycap.Net.Common
{
    using System;

    public static class Constants
    {
        public const string EndLine = "\r\n";
        public const string MimeDelimiter = "\r\n ";
        public const string NullByte = "\0";
        public const string PopERR = "-ERR";
        public const string PopHeaderEnd = "\r\n\r\n";
        public const string PopMessageEnd = "\r\n\r\n\r\n.\r\n";
        public const string PopOK = "+OK";
        public const string PopRetrMessageEnd = "\r\n.\r\n";
        public const string PopSALSContinue = "+ ";
        public const string RegEmail = @"(?:[\w\d\.!#$%&'*+\-/=?^_`{|}~-]+)@(?:[\w\d\.!#$%&'*+\-/=?^_`{|}~]+)(?:\.[a-zA-Z]{2,7})?";
    }
}

