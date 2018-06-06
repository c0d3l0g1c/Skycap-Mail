using System;
using System.Linq;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task to move a message.
    /// </summary>
    public class MoveMessagePrioritisedQueuedTask : PrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.MoveMessagePrioritisedQueuedTask class.
        /// </summary>
        /// <param name="moveMessage">The move message data.</param>
        public MoveMessagePrioritisedQueuedTask(MoveMessageEventArgs moveMessage)
            : base(PrioritisedQueuedTaskType.MoveMessage, moveMessage.Mailbox)
        { 
            // Initialise local variables
            MoveMessage = moveMessage;
        }

        /// <summary>
        /// Gets the move message data.
        /// </summary>
        public MoveMessageEventArgs MoveMessage
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
                MoveMessagePrioritisedQueuedTask moveMessagePrioritisedQueuedTask = obj as MoveMessagePrioritisedQueuedTask;
                if (moveMessagePrioritisedQueuedTask == null)
                    // Not a match
                    return false;
                // Return true if match
                return (moveMessagePrioritisedQueuedTask.Mailbox.FullName.Equals(this.Mailbox.FullName, StringComparison.OrdinalIgnoreCase)
                     && moveMessagePrioritisedQueuedTask.MoveMessage.DestinationMailbox.FullName.Equals(this.MoveMessage.DestinationMailbox.FullName, StringComparison.OrdinalIgnoreCase)
                     && string.Join("", moveMessagePrioritisedQueuedTask.MoveMessage.MessagePaths.Keys.Select(o => o.ToLower())).Equals(string.Join("", this.MoveMessage.MessagePaths.Keys.Select(o => o.ToLower())), StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.TaskType.GetHashCode() ^ this.Mailbox.FullName.ToLower().GetHashCode() ^ this.MoveMessage.DestinationMailbox.FullName.ToLower().GetHashCode() + string.Join("", this.MoveMessage.MessagePaths.Keys.Select(o => o.ToLower())).GetHashCode();
        }
    }
}
