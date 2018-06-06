using System;
using System.Runtime.Serialization;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the importance of an email.
    /// </summary>
    [DataContract]
    public enum MailImportance
    {
        /// <summary>
        /// Email is of low importance.
        /// </summary>
        [EnumMember]
        Low,
        /// <summary>
        /// Email is of normal importance.
        /// </summary>
        [EnumMember]
        Normal,
        /// <summary>
        /// Email is of high importance.
        /// </summary>
        [EnumMember]
        High,
    }
}
