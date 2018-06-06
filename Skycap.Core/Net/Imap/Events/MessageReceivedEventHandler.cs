namespace Skycap.Net.Imap.Events
{
    using Skycap.Net.Imap;
    using System;
    using System.Runtime.CompilerServices;

    public delegate void MessageReceivedEventHandler(ImapClient sender, ImapMessage message);
}

