namespace Skycap.Net.Imap.Events
{
    using System;
    using System.Runtime.CompilerServices;
    using Skycap.Net.Common;
    using Skycap.Net.Imap;

    public delegate void StateChangedEventHandler(ImapClient sender, Mailbox activeMailBox);
}

