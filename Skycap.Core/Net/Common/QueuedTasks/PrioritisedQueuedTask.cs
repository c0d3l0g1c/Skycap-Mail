using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised queued task.
    /// </summary>
    public abstract class PrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueueTask class.
        /// </summary>
        /// <param name="queuedTaskType">The queued task type.</param>
        /// <param name="mailbox">The mailbox.</param>
        public PrioritisedQueuedTask(PrioritisedQueuedTaskType queuedTaskType, Mailbox mailbox)
        {
            // Initialise local variables
            Priority = (int)queuedTaskType;
            TaskType = queuedTaskType;
            Mailbox = mailbox;
        }

        /// <summary>
        /// Gets the priority of the task.
        /// </summary>
        public int Priority
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the task type.
        /// </summary>
        public PrioritisedQueuedTaskType TaskType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the mailbox.
        /// </summary>
        public Mailbox Mailbox
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified Skycap.Net.Common.QueuedTasks.PrioritisedQueuedTask value.
        /// </summary>
        /// <param name="obj">An Skycap.Net.Common.QueuedTasks.PrioritisedQueuedTask value to compare to this instance.</param>
        /// <returns>true if obj has the same value as this instance; otherwise, false.</returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public abstract override int GetHashCode();
    }
}
