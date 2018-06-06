using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task to mark a message as flagged.
    /// </summary>
    public class MarkAsFlaggedPrioritisedQueuedTask : MessageMailboxPrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.Tasks.MarkAsFlaggedQueuedTask class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="mailbox">The mailbox.</param>
        public MarkAsFlaggedPrioritisedQueuedTask(StructuredMessage message, Mailbox mailbox)
            : base(PrioritisedQueuedTaskType.MarkAsFlagged, mailbox, message.Uid, message)
        {

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
                MarkAsFlaggedPrioritisedQueuedTask markAsFlaggedPrioritisedQueuedTask = obj as MarkAsFlaggedPrioritisedQueuedTask;
                if (markAsFlaggedPrioritisedQueuedTask == null)
                    // Not a match
                    return false;
                // Return true if match
                return (markAsFlaggedPrioritisedQueuedTask.Mailbox.FullName.Equals(this.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                     && markAsFlaggedPrioritisedQueuedTask.Message.Uid.Equals(this.Message.Uid, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.TaskType.GetHashCode() ^ this.Mailbox.FullName.ToLower().GetHashCode() ^ this.Message.Uid.ToLower().GetHashCode();
        }
    }
}
