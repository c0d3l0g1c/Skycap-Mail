using System.Runtime.Serialization;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the various enumerations for the navigation context.
    /// </summary>
    [DataContract]
    internal enum NavigationContext
    {
        /// <summary>
        /// Indicates navigation to compose a new mail.
        /// </summary>
        [EnumMember]
        New,
        /// <summary>
        /// Indicates navigation to edit an mail.
        /// </summary>
        [EnumMember]
        Edit,
        /// <summary>
        /// Indicates navigation to reply to sender.
        /// </summary>
        [EnumMember]
        Reply,
        /// <summary>
        /// Indicates navigation to reply to all.
        /// </summary>
        [EnumMember]
        ReplyToAll,
        /// <summary>
        /// Indicates navigation to forward.
        /// </summary>
        [EnumMember]
        Forward
    }
}
