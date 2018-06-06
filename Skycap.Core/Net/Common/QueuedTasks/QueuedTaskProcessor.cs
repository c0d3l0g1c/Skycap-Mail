using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents the queued task processor.
    /// </summary>
    public abstract class QueuedTaskProcessor<T, V>
        where T : IQueue<V>, new()
    {
        /// <summary>
        /// A value indicating whether queued tasks are currently being processed.
        /// </summary>
        private volatile bool _isRunning;
        /// <summary>
        /// A value indicating whether a queued task processor is currently busy.
        /// </summary>
        private volatile bool _isBusy;

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.QueuedTaskProcessor class.
        /// </summary>
        /// <param name="mailClient">The mail client.</param>
        public QueuedTaskProcessor(MailClient mailClient)
        { 
            // Initialise local variables
            MailClient = mailClient;
            Queue = new T();
        }

        /// <summary>
        /// Gets the mail client.
        /// </summary>
        public MailClient MailClient
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the queue to process.
        /// </summary>
        public T Queue
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a value indicating whether the queued task processor is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a queued task processor is busy.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
        }

        /// <summary>
        /// Starts processing the queued tasks.
        /// </summary>
        public async Task Start()
        {
            // Make sure it's not running
            if (_isRunning)
                await Stop();

            // Mark as running
            _isRunning = true;
            // Process task
            ProcessTask();
        }

        /// <summary>
        /// Stops processing the queued tasks.
        /// </summary>
        public async Task Stop()
        {
            // Mark as not running
            _isRunning = false;

            // Make sure there are no more tasks being processed
            while (_isBusy)
                await Task.Delay(5000);
        }

        /// <summary>
        /// Processes the specified queued task.
        /// </summary>
        /// <param name="task">The task.</param>
        public abstract Task ProcessTask(V task);

        /// <summary>
        /// Processes a queued task.
        /// </summary>
        private void ProcessTask()
        {
            Task.Factory.StartNew(async() =>
            {
                // Stores the prioritised queued task
                V prioritisedQueuedTask = default(V);

                // While IsProcessing = true
                while (IsRunning)
                {
                    // If a queued task is not available
                    if ((prioritisedQueuedTask = Queue.Peek()) == null)
                    {
                        await Task.Delay(10000);
                    }
                    // Else if a queued task is available
                    else
                    {
                        // If we are not busy
                        if (!_isBusy)
                        {
                            try
                            {
                                // Mark as busy
                                _isBusy = true;
                                // Process task
                                await ProcessTask(prioritisedQueuedTask);
                                // Dequeue task
                                Queue.Dequeue(prioritisedQueuedTask);
                            }
                            catch (Exception ex)
                            {
                                LogFile.Instance.LogError("", "", ex.ToString());
                            }
                            finally
                            {
                                // Mark as not busy
                                _isBusy = false;
                            }
                        }
                    }
                }
            });
        }
    }
}
