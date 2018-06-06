using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a prioritised message and/or mailbox queued task.
    /// </summary>
    public abstract class MessageMailboxPrioritisedQueuedTask : PrioritisedQueuedTask
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Common.Net.Tasks.MessageQueuedTask class.
        /// </summary>
        /// <param name="queuedTaskType">The type of queued task.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="uid">The unique id of the message.</param>
        public MessageMailboxPrioritisedQueuedTask(PrioritisedQueuedTaskType queuedTaskType, Mailbox mailbox, string uid)
            : base(queuedTaskType, mailbox)
        {

        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Common.Net.Tasks.MessageQueuedTask class.
        /// </summary>
        /// <param name="queuedTaskType">The type of queued task.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="uid">The unique id of the message.</param>
        /// <param name="message">The message.</param>
        public MessageMailboxPrioritisedQueuedTask(PrioritisedQueuedTaskType queuedTaskType, Mailbox mailbox, string uid, StructuredMessage message)
            : base(queuedTaskType, mailbox)
        {
            // Initialise local variables
            Uid = uid;
            Message = message;
        }

        /// <summary>
        /// Gets the unique id of the message.
        /// </summary>
        public string Uid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public StructuredMessage Message
        {
            get;
            private set;
        }
    }
}