namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Responses;
    using System;

    internal class LISTCommand : BaseListCommand
    {
        public LISTCommand(string referenceName, string mailboxName) : base("LIST", referenceName, mailboxName)
        {
            base.filter = new ResponseFilter(new string[] { "LIST" });
        }
    }
}

