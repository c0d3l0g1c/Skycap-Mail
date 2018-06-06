using System.Collections.ObjectModel;
using System.Runtime.Serialization;

using Skycap.Net.Common;

namespace Skycap.Notifications
{
    /// <summary>
    /// Represents the email toast notification context.
    /// </summary>
    [DataContract]
    public sealed class EmailToastNotificationContext
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Notifications.EmailToastNotificationContext class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="mailboxFullName">The mailbox full name.</param>
        /// <param name="uid">The message uid.</param>
        public EmailToastNotificationContext(string emailAddress, string mailboxFullName, string uid)
        { 
            // Initialise local variables
            EmailAddress = emailAddress;
            MailboxFullName = mailboxFullName;
            Uid = uid;
        }

        /// <summary>
        /// Gets the email address.
        /// </summary>
        [DataMember]
        public string EmailAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the mailbox full name.
        /// </summary>
        [DataMember]
        public string MailboxFullName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        [DataMember]
        public string Uid
        {
            get;
            private set;
        }
    }
}
