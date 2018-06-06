using System;
using System.Runtime.Serialization;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the various enumerations of the email flags.
    /// </summary>
    [DataContract]
    public enum EmailFlags
    {
        /// <summary>
        /// Indicates all emails.
        /// </summary>
        [EnumMember]
        All,
        /// <summary>
        /// Indicates read emails.
        /// </summary>
        [EnumMember]
        Read,
        /// <summary>
        /// Indicates unread emails.
        /// </summary>
        [EnumMember]
        Unread,
        /// <summary>
        /// Indicates flagged emails.
        /// </summary>
        [EnumMember]
        Flagged,
        /// <summary>
        /// Indicates high importance emails.
        /// </summary>
        [EnumMember]
        HighImportance,
        /// <summary>
        /// Indicates low importance emails.
        /// </summary>
        [EnumMember]
        LowImportance,
    }
}
