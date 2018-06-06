namespace Skycap.Net.Imap
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public enum EFlag
    {
        [EnumMember]
        NonStandart,
        [EnumMember]
        Deleted,
        [EnumMember]
        Seen,
        [EnumMember]
        Draft,
        [EnumMember]
        Answered,
        [EnumMember]
        Flagged,
        [EnumMember]
        Recent
    }
}

