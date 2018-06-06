using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents a first-in, first-out collection of outbox messages.
    /// </summary>
    public class OutboxQueue : IQueue<StructuredMessage>
    {
        /// <summary>
        /// The queue.
        /// </summary>
        private ConcurrentQueue<StructuredMessage> _queue;

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.OutboxQueue class.
        /// </summary>
        public OutboxQueue()
            : base()
        {
            // Initialise local variables
            _queue = new ConcurrentQueue<StructuredMessage>();
        }

        /// <summary>
        /// Removes the first occurence of a specific object from the System.Collections.Generic.Queue<T>.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        public void Dequeue(StructuredMessage item)
        {
            _queue.TryDequeue(out item);
        }

        /// <summary>
        /// Adds an object to the end of the System.Collections.Generic.Queue<T>.
        /// </summary>
        /// <param name="item">The object to add to the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
        public void Enqueue(StructuredMessage item)
        {
            _queue.Enqueue(item);
        }

        /// <summary>
        /// Returns the object at the beginning of the System.Collections.Generic.Queue<T> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the System.Collections.Generic.Queue<T>.</returns>
        public StructuredMessage Peek()
        {
            // Stores the message
            StructuredMessage message = null;

            // Try to peek
            _queue.TryPeek(out message);

            // Return message
            return message;
        }

        /// <summary>
        /// Gets the download message count.
        /// </summary>
        /// <param name="prioritisedQueuedTaskType">The prioritised queued task type.</param>
        public int GetQueuedTaskCount(PrioritisedQueuedTaskType prioritisedQueuedTaskType)
        {
            if (prioritisedQueuedTaskType == PrioritisedQueuedTaskType.DownloadMessage)
                return _queue.Count;
            return 0;
        }
    }
}
