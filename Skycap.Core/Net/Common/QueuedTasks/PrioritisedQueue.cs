using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    ///  Represents a prioritised first-in, first-out collection of objects.
    /// </summary>
    public class PrioritisedQueue : IQueue<PrioritisedQueuedTask>
    {
        /// <summary>
        /// The prioritised queue.
        /// </summary>
        private volatile SortedDictionary<int, List<PrioritisedQueuedTask>> _queue;

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.PrioritisedQueue class.
        /// </summary>
        public PrioritisedQueue()
        { 
            // Initialise local variables
            _queue = new SortedDictionary<int, List<PrioritisedQueuedTask>>();

            // Create the priorities with blank queues
            foreach (int priority in Enum.GetValues(typeof(PrioritisedQueuedTaskType)))
                _queue.Add(priority, new List<PrioritisedQueuedTask>(1000));
        }

        /// <summary>
        /// Gets the processing mailbox.
        /// </summary>
        public Mailbox ProcessingMailbox
        {
            get;
            set;
        }

        /// <summary>
        /// Adds an object to the end of the System.Collections.Generic.Queue<T>.
        /// </summary>
        /// <param name="item">The object to add to the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
        public void Enqueue(PrioritisedQueuedTask item)
        {
            lock (_queue)
            {
                if (!_queue[item.Priority].Contains(item))
                    _queue[item.Priority].Add(item);
            }
        }

        /// <summary>
        /// Returns the object at the beginning of the System.Collections.Generic.Queue<T> without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the System.Collections.Generic.Queue<T>.</returns>
        public PrioritisedQueuedTask Peek()
        {
            try
            {
                lock (_queue)
                {
                    // Loop through each item in the queue
                    foreach (KeyValuePair<int, List<PrioritisedQueuedTask>> queueEntry in _queue)
                    {
                        // If there are items in the queue
                        foreach (PrioritisedQueuedTask prioritisedQueuedTask in queueEntry.Value)
                        {
                            // If DownloadMessage
                            if (prioritisedQueuedTask.TaskType == PrioritisedQueuedTaskType.DownloadMessage)
                            {
                                // Get the processing mailbox count
                                int processingMailboxCount = (ProcessingMailbox == null ? 0 : queueEntry.Value.Where(o => o.Mailbox.FullName.Equals(ProcessingMailbox.FullName, StringComparison.OrdinalIgnoreCase)).Count());

                                // Get the most recent message for this mailbox
                                return queueEntry.Value.Where(o => (ProcessingMailbox == null || o.Mailbox.FullName.Equals(ProcessingMailbox.FullName, StringComparison.OrdinalIgnoreCase)) || processingMailboxCount == 0)
                                       .Cast<DownloadMessagePrioritisedQueuedTask>()
                                       .OrderBy(o => (int)o.Mailbox.Folder)
                                       .ThenBy(o => o.Mailbox.FullName)
                                       .ThenByDescending(o => o.MessageProgress.StatisticInfo.SerialNumber)
                                       .FirstOrDefault();
                            }
                            // Else if anything else
                            else
                                return prioritisedQueuedTask;
                        }
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError("", "", ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Removes the first occurence of a specific object from the System.Collections.Generic.Queue<T>.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        public void Dequeue(PrioritisedQueuedTask item)
        {
            lock (_queue)
            {
                _queue[item.Priority].Remove(item);
            }
        }

        /// <summary>
        /// Gets the download message count.
        /// </summary>
        /// <param name="prioritisedQueuedTaskType">The prioritised queued task type.</param>
        public int GetQueuedTaskCount(PrioritisedQueuedTaskType prioritisedQueuedTaskType)
        {
            lock (_queue)
            {
                return _queue[(int)prioritisedQueuedTaskType].Count;
            }
        }
    }
}
