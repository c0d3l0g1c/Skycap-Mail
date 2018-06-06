using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task to download a message.
    /// </summary>
    public class DownloadMessagePrioritisedQueuedTask : MessageMailboxPrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Tasks.DownloadMessageQueuedTask class.
        /// </summary>
        /// <param name="messageProgress">The message progress.</param>
        public DownloadMessagePrioritisedQueuedTask(MessageProgressEventArgs messageProgress)
            : base(PrioritisedQueuedTaskType.DownloadMessage, messageProgress.Mailbox, messageProgress.Uid, messageProgress.Message)
        {
            // Initialise local variables
            MessageProgress = messageProgress;
        }

        /// <summary>
        /// Gets or sets the message progress.
        /// </summary>
        public MessageProgressEventArgs MessageProgress
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified Skycap.Net.Common.QueuedTasks.PrioritisedQueuedTask value.
        /// </summary>
        /// <param name="obj">An Skycap.Net.Common.QueuedTasks.PrioritisedQueuedTask value to compare to this instance.</param>
        /// <returns>true if obj has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            // If obj is not an instance
            if (obj == null)
                return false;
            // Else if obj is an instance
            else
            {
                // If obj is not an instance of this type
                DownloadMessagePrioritisedQueuedTask downloadMessagePrioritisedQueuedTask = obj as DownloadMessagePrioritisedQueuedTask;
                if (downloadMessagePrioritisedQueuedTask == null)
                    // Not a match
                    return false;
                // Return true if match
                return (downloadMessagePrioritisedQueuedTask.Mailbox.FullName.Equals(this.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                     && downloadMessagePrioritisedQueuedTask.MessageProgress.StatisticInfo.UniqueNumber.Equals(this.MessageProgress.StatisticInfo.UniqueNumber, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.TaskType.GetHashCode() ^ this.Mailbox.FullName.ToLower().GetHashCode() ^ this.MessageProgress.StatisticInfo.UniqueNumber.ToLower().GetHashCode();
        }
    }
}
