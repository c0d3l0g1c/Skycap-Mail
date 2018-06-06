namespace Skycap.Net.Common.MessageParts
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public enum EPartType
    {
        Content = 2,
        Message = 4,
        Multi = 1,
        Text = 3
    }
}

