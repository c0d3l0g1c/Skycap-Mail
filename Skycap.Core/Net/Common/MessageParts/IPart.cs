namespace Skycap.Net.Common.MessageParts
{
    using Skycap.Net.Common;
    using System;
    using System.Runtime.Serialization;

    public interface IPart
    {
        [DataMember]
        MIMEHeader Header { get; set; }

        EPartType Type { get; }
    }
}

