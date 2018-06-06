namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Responses;
    using System;

    internal class LSUBCommand : BaseListCommand
    {
        public LSUBCommand(string referenceName, string mailboxName) : base("LSUB", referenceName, mailboxName)
        {
            base.filter = new ResponseFilter(new string[] { "LSUB" });
        }
    }
}

