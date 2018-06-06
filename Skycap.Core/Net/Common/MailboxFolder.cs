using System;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the various enumerations for mailbox folders.
    /// </summary>
    public enum MailboxFolders
    {
        /// <summary>
        /// Represents emails that have been received.
        /// </summary>
        Inbox,
        /// <summary>
        /// Represents emails that are in progress.
        /// </summary>
        Drafts,
        /// <summary>
        /// Represents emails that are ready to be sent out.
        /// </summary>
        Outbox,
        /// <summary>
        /// Represents emails that have been sent out.
        /// </summary>
        SentItems,
        /// <summary>
        /// Represents junk or spam mail.
        /// </summary>
        JunkMail,
        /// <summary>
        /// Represents emails that have been deleted.
        /// </summary>
        DeletedItems,
        /// <summary>
        /// Represents emails categorised by some custom folder.
        /// </summary>
        Folder,
    }
}
