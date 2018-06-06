using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents the various enumerations for item type.
    /// </summary>
    public enum PrioritisedQueuedTaskType
    {
        /// <summary>
        /// Indicates a queued task to mark a message as deleted.
        /// </summary>
        MarkAsDeleted = 0,
        /// <summary>
        /// Indicates a queued task to mark a message as undeleted.
        /// </summary>
        MarkAsUndeleted = 1,
        /// <summary>
        /// Indicates a queued task to delete a message.
        /// </summary>
        DeleteMessage = 2,
        /// <summary>
        /// Indicates a queued task to save to drafts.
        /// </summary>
        SaveToDrafts = 3,
        /// <summary>
        /// Indicates a queued task to mark a message as read.
        /// </summary>
        MarkAsRead = 4,
        /// <summary>
        /// Indicates a queued task to mark a message as unread.
        /// </summary>
        MarkAsUnread = 5,
        /// <summary>
        /// Indicates a queued task to mark a message as flagged.
        /// </summary>
        MarkAsFlagged = 6,
        /// <summary>
        /// Indicates a queued task to mark a message as unflagged.
        /// </summary>
        MarkAsUnflagged = 7,
        /// <summary>
        /// Indicates a queued task to move a message.
        /// </summary>
        MoveMessage = 8,
        /// <summary>
        /// Indicates a queued task to add a mailbox.
        /// </summary>
        AddMailbox = 9,
        /// <summary>
        /// Indicates a queued task to rename a mailbox.
        /// </summary>
        RenameMailbox = 10,
        /// <summary>
        /// Indicates a queued task to remove a mailbox.
        /// </summary>
        RemoveMailbox = 11,
        /// <summary>
        /// Indicates a queued task to download a message.
        /// </summary>
        DownloadMessage = 12,
    }
}