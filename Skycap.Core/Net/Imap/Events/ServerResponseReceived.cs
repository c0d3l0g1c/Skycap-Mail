namespace Skycap.Net.Imap.Events
{
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Runtime.CompilerServices;

    public delegate void ServerResponseReceived(object sender, IMAP4Response response);
}

