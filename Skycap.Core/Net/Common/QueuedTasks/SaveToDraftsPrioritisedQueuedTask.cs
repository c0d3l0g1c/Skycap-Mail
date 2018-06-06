using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task to save to drafts.
    /// </summary>
    public class SaveToDraftsPrioritisedQueuedTask : MessageMailboxPrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.SaveToDraftsPrioritisedQueuedTask class.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message.</param>
        public SaveToDraftsPrioritisedQueuedTask(Mailbox mailbox, StructuredMessage message)
            : base(PrioritisedQueuedTaskType.SaveToDrafts, mailbox, message.Uid, message)
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
                SaveToDraftsPrioritisedQueuedTask saveToDraftsPrioritisedQueuedTask = obj as SaveToDraftsPrioritisedQueuedTask;
                if (saveToDraftsPrioritisedQueuedTask == null)
                    // Not a match
                    return false;
                // Return true if match
                return (saveToDraftsPrioritisedQueuedTask.Mailbox.FullName.Equals(this.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                     && saveToDraftsPrioritisedQueuedTask.Message.Header.MessageID.Equals(this.Message.Header.MessageID, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.TaskType.GetHashCode() ^ this.Mailbox.FullName.ToLower().GetHashCode() ^ this.Message.Header.MessageID.ToLower().GetHashCode();
        }
    }
}
