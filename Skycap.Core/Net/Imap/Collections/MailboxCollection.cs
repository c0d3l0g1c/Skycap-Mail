namespace Skycap.Net.Imap.Collections
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Skycap.Net.Common;

    [CollectionDataContract]
    public class MailboxCollection : List<Mailbox>
    {
    }
}

