namespace Skycap.Net.Imap.Responses
{
    using System;
    using System.Collections.Generic;

    public class ResponseFilter : List<string>
    {
        public ResponseFilter()
        {
        }

        public ResponseFilter(IEnumerable<string> commandNames) : base(commandNames)
        {
        }
    }
}

