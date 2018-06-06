using System;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Implements the methods required for queuing.
    /// </summary>
    public interface IQueue<T>
    {
        /// <summary>
        /// Removes the first occurence of a specific object from the System.Collections.Generic.Queue<T>.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        void Dequeue(T item);
        /// <summary>
        /// Adds an object to the end of the System.Collections.Generic.Queue<T>.
        /// </summary>
        /// <param name="item">The object to add to the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
        void Enqueue(T item);
        /// <summary>
        /// Returns the object at the beginning of the System.Collections.Generic.Queue<T> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the System.Collections.Generic.Queue<T>.</returns>
        T Peek();
        /// <summary>
        /// Gets the download message count.
        /// </summary>
        /// <param name="prioritisedQueuedTaskType">The prioritised queued task type.</param>
        int GetQueuedTaskCount(PrioritisedQueuedTaskType prioritisedQueuedTaskType);
    }
}
