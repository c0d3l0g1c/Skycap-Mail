namespace Skycap.Net.Imap.Collections
{
    using Skycap.Net.Imap;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [CollectionDataContract]
    public class MessageFlagCollection : List<MessageFlag>
    {
        public bool Contains(EFlag flag)
        {
            return base.Contains(new MessageFlag(flag));
        }

        public bool Contains(string flag)
        {
            return base.Contains(new MessageFlag(flag));
        }
    }
}

