namespace Skycap.Net.Common
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public enum EContentTransferEncoding
    {
        [EnumMember] 
        SevenBit,
        [EnumMember]
        EightBit,
        [EnumMember]
        Binary,
        [EnumMember]
        Base64,
        [EnumMember]
        QuotedPrintable
    }
}

