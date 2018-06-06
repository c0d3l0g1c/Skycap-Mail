namespace Skycap.Net.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message) : base(message)
        {
        }
    }
}

