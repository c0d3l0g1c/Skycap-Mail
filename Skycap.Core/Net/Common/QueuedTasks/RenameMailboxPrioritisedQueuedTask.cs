using System;
using System.Collections.Generic;
using System.Linq;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task to edit a mailbox.
    /// </summary>
    public class RenameMailboxPrioritisedQueuedTask : PrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.RenameMailboxPrioritisedQueuedTask class.
        /// </summary>
        /// <param name="renamedMailbox">The renamed mailboxes.</param>
        public RenameMailboxPrioritisedQueuedTask(Dictionary<Mailbox, Mailbox> renamedMailboxes)
            : base(PrioritisedQueuedTaskType.RenameMailbox, renamedMailboxes.Keys.First())
        { 
            // Initialise local variables
            RenamedMailboxes = renamedMailboxes;
        }

        /// <summary>
        /// Gets the renamed mailboxes.
        /// </summary>
        public Dictionary<Mailbox, Mailbox> RenamedMailboxes
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
                RenameMailboxPrioritisedQueuedTask renameMailboxPrioritisedQueuedTask = obj as RenameMailboxPrioritisedQueuedTask;
                if (renameMailboxPrioritisedQueuedTask == null)
                    // Not a match
                    return false;
                // Return true if match
                return (string.Join("", renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Keys.Select(o => o.FullName.ToLower())).Equals(string.Join("", this.RenamedMailboxes.Values.Select(o => o.FullName.ToLower())), StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.TaskType.GetHashCode() ^ this.Mailbox.FullName.ToLower().GetHashCode() ^ string.Join("", this.RenamedMailboxes.Keys.Select(o => o.FullName.ToLower())).GetHashCode();
        }
    }
}
