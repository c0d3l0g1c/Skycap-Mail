using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents the outbox queued task processor.
    /// </summary>
    public class OutboxQueuedTaskProcessor : QueuedTaskProcessor<OutboxQueue, StructuredMessage>
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.OutboxQueuedTaskProcessor class.
        /// </summary>
        /// <param name="mailClient">The mail client.</param>
        public OutboxQueuedTaskProcessor(MailClient mailClient)
            : base(mailClient)
        {

        }

        /// <summary>
        /// Processes the specified queued task.
        /// </summary>
        /// <param name="task">The task.</param>
        public async override Task ProcessTask(StructuredMessage task)
        {
            await MailClient.ProcessSendMessage(task, false);
        }
    }
}
