using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Skycap.Net.Common.QueuedTasks
{
    /// <summary>
    /// Represents the method that processes a prioritised queued task.
    /// </summary>
    /// <typeparam name="T">The System.Type of the PrioritisedQueuedTask.</typeparam>
    /// <param name="prioritisedQueuedTask">The prioritised queued task.</param>
    public delegate void ProcessPrioritisedQueuedTask<T>(T prioritisedQueuedTask)
        where T : PrioritisedQueuedTask;

    /// <summary>
    /// Represents the queued task processor.
    /// </summary>
    public class PrioritisedQueuedTaskProcessor : QueuedTaskProcessor<PrioritisedQueue, PrioritisedQueuedTask>
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.QueuedTasks.QueuedTaskProcessor class.
        /// </summary>
        /// <param name="mailClient">The mail client.</param>
        public PrioritisedQueuedTaskProcessor(MailClient mailClient)
            : base(mailClient)
        {

        }

        /// <summary>
        /// Gets the processing mailbox.
        /// </summary>
        public Mailbox ProcessingMailbox
        {
            get
            {
                return Queue.ProcessingMailbox;
            }
            set
            {
                Queue.ProcessingMailbox = value;
            }
        }

        /// <summary>
        /// Processes the specified queued task.
        /// </summary>
        /// <param name="task">The task.</param>
        public async override Task ProcessTask(PrioritisedQueuedTask task)
        {
            // Determine what task type we are dealing with
            switch (task.TaskType)
            {
                // If SaveToDrafts
                case PrioritisedQueuedTaskType.SaveToDrafts:
                    await MailClient.ProcessSaveToDraftsPrioritisedQueuedTask((SaveToDraftsPrioritisedQueuedTask)task);
                    break;

                // If MarkAsRead
                case PrioritisedQueuedTaskType.MarkAsRead:
                    await MailClient.ProcessMarkAsReadPrioritisedQueuedTask((MarkAsReadPrioritisedQueuedTask)task);
                    break;

                // If MarkAsUnread
                case PrioritisedQueuedTaskType.MarkAsUnread:
                    await MailClient.ProcessMarkAsUnreadPrioritisedQueuedTask((MarkAsUnreadPrioritisedQueuedTask)task);
                    break;

                // If MarkAsDeleted
                case PrioritisedQueuedTaskType.MarkAsDeleted:
                    await MailClient.ProcessMarkAsDeletedPrioritisedQueuedTask((MarkAsDeletedPrioritisedQueuedTask)task);
                    break;

                // If MarkAsUndeleted
                case PrioritisedQueuedTaskType.MarkAsUndeleted:
                    await MailClient.ProcessMarkAsUndeletedPrioritisedQueuedTask((MarkAsUndeletedPrioritisedQueuedTask)task);
                    break;

                // If MarkAsFlagged
                case PrioritisedQueuedTaskType.MarkAsFlagged:
                    await MailClient.ProcessMarkAsFlaggedPrioritisedQueuedTask((MarkAsFlaggedPrioritisedQueuedTask)task);
                    break;

                // If MarkAsUnflagged
                case PrioritisedQueuedTaskType.MarkAsUnflagged:
                    await MailClient.ProcessMarkAsUnflaggedPrioritisedQueuedTask((MarkAsUnflaggedPrioritisedQueuedTask)task);
                    break;

                // If DeleteMessage
                case PrioritisedQueuedTaskType.DeleteMessage:
                    await MailClient.ProcessDeleteMessagePrioritisedQueuedTask((DeleteMessagePrioritisedQueuedTask)task);
                    break;

                // If MoveMessage
                case PrioritisedQueuedTaskType.MoveMessage:
                    await MailClient.ProcessMoveMessagePrioritisedQueuedTask((MoveMessagePrioritisedQueuedTask)task);
                    break;

                // If NewMailbox
                case PrioritisedQueuedTaskType.AddMailbox:
                    await MailClient.ProcessAddMailboxPrioritisedQueuedTask((AddMailboxPrioritisedQueuedTask)task);
                    break;

                // If EditMailbox
                case PrioritisedQueuedTaskType.RenameMailbox:
                    await MailClient.ProcessRenameMailboxPrioritisedQueuedTask((RenameMailboxPrioritisedQueuedTask)task);
                    break;

                // If RemoveMailbox
                case PrioritisedQueuedTaskType.RemoveMailbox:
                    await MailClient.ProcessRemoveMailboxPrioritisedQueuedTask((RemoveMailboxPrioritisedQueuedTask)task);
                    break;

                // If DownloadMessage
                case PrioritisedQueuedTaskType.DownloadMessage:
                    await MailClient.ProcessDownloadMessagePrioritisedQueuedTask((DownloadMessagePrioritisedQueuedTask)task);
                    break;
            }
        }
    }
}
