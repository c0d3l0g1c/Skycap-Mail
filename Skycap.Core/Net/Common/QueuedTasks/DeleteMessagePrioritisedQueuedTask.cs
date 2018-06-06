using System;
using System.Linq;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task to delete a message.
    /// </summary>
    public class DeleteMessagePrioritisedQueuedTask : PrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.Tasks.DeleteMessageQueuedTask class.
        /// </summary>
        /// <param name="uid">The unique id of the message.</param>
        public DeleteMessagePrioritisedQueuedTask(DeleteMessageEventArgs deleteMessages)
            : base(PrioritisedQueuedTaskType.DeleteMessage, deleteMessages.Mailbox)
        {
            // Initialise local variables
            DeleteMessage = deleteMessages;
        }

        /// <summary>
        /// Gets the messages to delete.
        /// </summary>
        public DeleteMessageEventArgs DeleteMessage
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
                DeleteMessagePrioritisedQueuedTask deleteMessagePrioritisedQueuedTask = obj as DeleteMessagePrioritisedQueuedTask;
                if (deleteMessagePrioritisedQueuedTask == null)
                    // Not a match
                    return false;
                // Return true if match
                return (deleteMessagePrioritisedQueuedTask.Mailbox.FullName.Equals(this.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                     && string.Join("", deleteMessagePrioritisedQueuedTask.DeleteMessage.MessagePaths.Keys.Select(o => o.ToLower())).Equals(string.Join("", this.DeleteMessage.MessagePaths.Keys.Select(o => o.ToLower())), StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.TaskType.GetHashCode() ^ this.Mailbox.FullName.ToLower().GetHashCode() ^ string.Join("", this.DeleteMessage.MessagePaths.Keys.Select(o => o.ToLower())).GetHashCode();
        }
    }
}
