using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task to mark a message as deleted.
    /// </summary>
    public class MarkAsDeletedPrioritisedQueuedTask : MessageMailboxPrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.Tasks.MarkAsDeletedQueuedTask class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        public MarkAsDeletedPrioritisedQueuedTask(StructuredMessage message, Mailbox mailbox, Mailbox destinationMailbox)
            : base(PrioritisedQueuedTaskType.MarkAsDeleted, mailbox, message.Uid, message)
        {
            // Initialise local variables
            DestinationMailbox = destinationMailbox;
        }

        /// <summary>
        /// Gets the destination mailbox.
        /// </summary>
        public Mailbox DestinationMailbox
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
                MarkAsDeletedPrioritisedQueuedTask markAsDeletedPrioritisedQueuedTask = obj as MarkAsDeletedPrioritisedQueuedTask;
                if (markAsDeletedPrioritisedQueuedTask == null)
                    // Not a match
                    return false;
                // Return true if match
                return (markAsDeletedPrioritisedQueuedTask.Mailbox.FullName.Equals(this.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                     && markAsDeletedPrioritisedQueuedTask.Message.Uid.Equals(this.Message.Uid, StringComparison.OrdinalIgnoreCase));
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
