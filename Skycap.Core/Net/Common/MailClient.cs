using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Skycap.Data;
using Skycap.IO;
using Skycap.Net.Common.Collections;
using Skycap.Net.Common.Configurations;
using Skycap.Net.Common.QueuedTasks;
using Skycap.Net.Imap;
using Skycap.Net.Pop3;
using Skycap.Net.Smtp;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents an abstract mail client.
    /// </summary>
    public abstract class MailClient
    {
        #region Events

        /// <summary>
        /// Occurs when a mail client is connected to a mail server.
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// Occurs when a mail client attempts to log in.
        /// </summary>
        public event EventHandler LoggingIn;
        /// <summary>
        /// Occurs when a mail client log in attempt succeeds.
        /// </summary>
        public event EventHandler LoggedIn;
        /// <summary>
        /// Occurs when a mail client log in attempt fails.
        /// </summary>
        public event EventHandler<FailedEventArgs> LoginFailed;
        /// <summary>
        /// Occurs when a mail client attempts to update mailbox tree.
        /// </summary>
        public event EventHandler<MailboxTreeEventArgs> UpdatingMailboxTree;
        /// <summary>
        /// Occurs when a mail client update mailbox tree attempt succeeds.
        /// </summary>
        public event EventHandler<MailboxTreeEventArgs> UpdatedMailboxTree;
        /// <summary>
        /// Occurs when a mail client update mailbox tree attempt fails.
        /// </summary>
        public event EventHandler<MailboxTreeFailedEventArgs> UpdateMailboxTreeFailed;
        /// <summary>
        /// Occurs when a mail client starts a restore of all messages in a mailbox.
        /// </summary>
        public event EventHandler<MailboxEventArgs> RestoreStarted;
        /// <summary>
        /// Occurs when a mail client attempts to restore a message.
        /// </summary>
        public event EventHandler<MessageProgressEventArgs> RestoringMessage;
        /// <summary>
        /// Occurs when a mail client message restore attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> RestoredMessage;
        /// <summary>
        /// Occurs when a mail client message restore attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> RestoreMessageFailed;
        /// <summary>
        /// Occurs when a mail client completes a restore of all messages in a mailbox.
        /// </summary>
        public event EventHandler<MailboxEventArgs> RestoreCompleted;
        /// <summary>
        /// Occurs when a mail client starts a download of all messages in a mailbox.
        /// </summary>
        public event EventHandler<MailboxEventArgs> DownloadStarted;
        /// <summary>
        /// Occurs when a mail client attempts to download a message.
        /// </summary>
        public event EventHandler<MessageProgressEventArgs> DownloadingMessage;
        /// <summary>
        /// Occurs when a mail client message download attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> DownloadedMessage;
        /// <summary>
        /// Occurs when a mail client message download attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> DownloadMessageFailed;
        /// <summary>
        /// Occurs when a mail client completes a download of all messages in a mailbox.
        /// </summary>
        public event EventHandler<MailboxEventArgs> DownloadCompleted;
        /// <summary>
        /// Occurs when a mail client message update attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> UpdatedMessage;
        /// <summary>
        /// Occurs when a mail client attempts to mark a message as read.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkingMessageAsRead;
        /// <summary>
        /// Occurs when a mail client mark a message as read attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkMessageAsRead;
        /// <summary>
        /// Occurs when a mail client mark a message as read attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> MarkMessageAsReadFailed;
        /// <summary>
        /// Occurs when a mail client attempts to mark a message as unread.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkingMessageAsUnread;
        /// <summary>
        /// Occurs when a mail client mark a message as unread attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkMessageAsUnread;
        /// <summary>
        /// Occurs when a mail client mark a message as unread attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> MarkMessageAsUnreadFailed;
        /// <summary>
        /// Occurs when a mail client attempts to mark a message as deleted.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkingMessageAsDeleted;
        /// <summary>
        /// Occurs when a mail client mark a message as deleted attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkMessageAsDeleted;
        /// <summary>
        /// Occurs when a mail client mark a message as deleted attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> MarkMessageAsDeletedFailed;
        /// <summary>
        /// Occurs when a mail client attempts to mark a message as undeleted.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkingMessageAsUndeleted;
        /// <summary>
        /// Occurs when a mail client mark a message as undeleted attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkMessageAsUndeleted;
        /// <summary>
        /// Occurs when a mail client mark a message as undeleted attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> MarkMessageAsUndeletedFailed;
        /// <summary>
        /// Occurs when a mail client attempts to mark a message as flagged.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkingMessageAsFlagged;
        /// <summary>
        /// Occurs when a mail client mark a message as flagged attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkMessageAsFlagged;
        /// <summary>
        /// Occurs when a mail client mark a message as flagged attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> MarkMessageAsFlaggedFailed;
        /// <summary>
        /// Occurs when a mail client attempts to mark a message as unflagged.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkingMessageAsUnflagged;
        /// <summary>
        /// Occurs when a mail client mark a message as unflagged attempt succeeds.
        /// </summary>
        public event EventHandler<MessageEventArgs> MarkMessageAsUnflagged;
        /// <summary>
        /// Occurs when a mail client mark a message as unflagged attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> MarkMessageAsUnflaggedFailed;
        /// <summary>
        /// Occurs when a mail client attempts to move a message.
        /// </summary>
        public event EventHandler<MoveMessageEventArgs> MovingMessage;
        /// <summary>
        /// Occurs when a mail client move message attempt succeeds.
        /// </summary>
        public event EventHandler<MoveMessageEventArgs> MovedMessage;
        /// <summary>
        /// Occurs when a mail client move message attempt fails.
        /// </summary>
        public event EventHandler<MessageFailedEventArgs> MoveMessageFailed;
        /// <summary>
        /// Occurs when a mail client attempts to delete a message.
        /// </summary>
        public event EventHandler<DeleteMessageEventArgs> DeletingMessage;
        /// <summary>
        /// Occurs when a mail client delete message attempt succeeds.
        /// </summary>
        public event EventHandler<DeleteMessageEventArgs> DeletedMessage;
        /// <summary>
        /// Occurs when a mail client delete message attempt fails.
        /// </summary>
        public event EventHandler<DeleteMessageFailedEventArgs> DeleteMessageFailed;
        /// <summary>
        /// Occurs when a mail client attempts to add a mailbox.
        /// </summary>
        public event EventHandler<MailboxEventArgs> AddingMailbox;
        /// <summary>
        /// Occurs when a mail client add mailbox attempt succeeds.
        /// </summary>
        public event EventHandler<MailboxEventArgs> AddedMailbox;
        /// <summary>
        /// Occurs when a mail client add mailbox attempt fails.
        /// </summary>
        public event EventHandler<MailboxFailedEventArgs> AddMailboxFailed;
        /// <summary>
        /// Occurs when a mail client attempts to rename a mailbox.
        /// </summary>
        public event EventHandler<RenameMailboxEventArgs> RenamingMailbox;
        /// <summary>
        /// Occurs when a mail client rename mailbox attempt succeeds.
        /// </summary>
        public event EventHandler<RenameMailboxEventArgs> RenamedMailbox;
        /// <summary>
        /// Occurs when a mail client rename mailbox attempt fails.
        /// </summary>
        public event EventHandler<RenameMailboxFailedEventArgs> RenameMailboxFailed;
        /// <summary>
        /// Occurs when a mail client attempts to remove a mailbox.
        /// </summary>
        public event EventHandler<MailboxEventArgs> RemovingMailbox;
        /// <summary>
        /// Occurs when a mail client remove mailbox attempt succeeds.
        /// </summary>
        public event EventHandler<MailboxEventArgs> RemovedMailbox;
        /// <summary>
        /// Occurs when a mail client remove mailbox attempt fails.
        /// </summary>
        public event EventHandler<MailboxFailedEventArgs> RemoveMailboxFailed;
        /// <summary>
        /// Occurs when a mail client attempts to save to drafts.
        /// </summary>
        public event EventHandler<SaveToDraftsEventArgs> SavingToDrafts;
        /// <summary>
        /// Occurs when a mail client save to drafts attempt succeeds.
        /// </summary>
        public event EventHandler<SaveToDraftsEventArgs> SavedToDrafts;
        /// <summary>
        /// Occurs when a mail client save to drafts attempt fails.
        /// </summary>
        public event EventHandler<SaveToDraftsFailedEventArgs> SaveToDraftsFailed;
        /// <summary>
        /// Occurs when a mail client attempts to send a message.
        /// </summary>
        public event EventHandler<SendMessageEventArgs> SendingMessage;
        /// <summary>
        /// Occurs when a mail client send message attempt succeeds.
        /// </summary>
        public event EventHandler<SendMessageEventArgs> SentMessage;
        /// <summary>
        /// Occurs when a mail client send message attempt fails.
        /// </summary>
        public event EventHandler<SendMessageFailedEventArgs> SendMessageFailed;
        /// <summary>
        /// Occurs when a mail client attempts to log out.
        /// </summary>
        public event EventHandler LoggingOut;
        /// <summary>
        /// Occurs when a mail client log out attempt succeeds.
        /// </summary>
        public event EventHandler LoggedOut;
        /// <summary>
        /// Occurs when a mail client log out attempt fails.
        /// </summary>
        public event EventHandler<FailedEventArgs> LogoutFailed;
        /// <summary>
        /// Occurs when a mail client is disconnected from a mail server.
        /// </summary>
        public event EventHandler Disconnected;

        #endregion

        #region Constants

        /// <summary>
        /// The mailbox folder prefix.
        /// </summary>
        public const char MailboxFolderPrefix = '╠';
        /// <summary>
        /// The message folder prefix.
        /// </summary>
        public const char MessageFolderPrefix = '╟';
        /// <summary>
        /// The mailbox file name.
        /// </summary>
        public const string MailboxFilename = "Mailbox.dat";
        /// <summary>
        /// The uids file name.
        /// </summary>
        public const string UidsFilename = "Uids.dat";

        #endregion

        #region Variables

        /// <summary>
        /// The mailbox tree.
        /// </summary>
        private ObservableCollection<Mailbox> _mailboxTree;
        /// <summary>
        /// The account settings data.
        /// </summary>
        private AccountSettingsData _accountSettingsData;
        /// <summary>
        /// The smtp client.
        /// </summary>
        private SmtpClient smtpClient;
        /// <summary>
        /// The background timer to check for new emails.
        /// </summary>
        private BackgroundTimer backgroundTimer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MailClient class.
        /// Runs only once.
        /// </summary>
        static MailClient()
        {
            SkycapMailSmtpClient = new SmtpClient("mail.codelogic.co.za", 25, "skycapmail@codelogic.co.za", "$kyc@p", GetEmailServiceProviderSsl(false, EmailServiceProvider.Other), EAuthenticationType.Auto);
        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.MailClient class.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        public MailClient(AccountSettingsData accountSettingsData)
        {
            // Initialise local variables
            AccountSettingsData = accountSettingsData;
            ((INotifyPropertyChanged)AccountSettingsData).PropertyChanged += AccountSettingsData_PropertyChanged;
            backgroundTimer = new BackgroundTimer();
            backgroundTimer.Tick += backgroundTimer_Tick;
            smtpClient = new SmtpClient(accountSettingsData.OutgoingMailServer, accountSettingsData.OutgoingMailServerPort, accountSettingsData.SendUserName, accountSettingsData.SendPassword, GetEmailServiceProviderSsl(accountSettingsData.IsOutgoingMailServerSsl, accountSettingsData.EmailServiceProvider), accountSettingsData.OutgoingMailServerRequiresAuthentication ? EAuthenticationType.Auto : EAuthenticationType.None);
            TaskProcessor = new PrioritisedQueuedTaskProcessor(this);
            OutboxTaskProcessor = new OutboxQueuedTaskProcessor(this);
            MailboxRestoreStatus = new ConcurrentDictionary<string, MailboxActionState>(StringComparer.OrdinalIgnoreCase);
            MailboxDownloadStatus = new ConcurrentDictionary<string, MailboxActionState>(StringComparer.OrdinalIgnoreCase);
            MailboxProgressStatus = new ConcurrentDictionary<string, MessageProgressEventArgs>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Skycap Mail smtp client.
        /// </summary>
        private static SmtpClient SkycapMailSmtpClient
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the prioritised queued task processor.
        /// </summary>
        protected PrioritisedQueuedTaskProcessor TaskProcessor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the outbox queued task processor.
        /// </summary>
        protected OutboxQueuedTaskProcessor OutboxTaskProcessor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the account settings data.
        /// </summary>
        public AccountSettingsData AccountSettingsData
        {
            get
            {
                return _accountSettingsData;
            }
            set
            {
                _accountSettingsData = value;
                smtpClient = new SmtpClient(_accountSettingsData.OutgoingMailServer, _accountSettingsData.OutgoingMailServerPort, _accountSettingsData.SendUserName, _accountSettingsData.SendPassword, GetEmailServiceProviderSsl(_accountSettingsData.IsOutgoingMailServerSsl, _accountSettingsData.EmailServiceProvider), _accountSettingsData.OutgoingMailServerRequiresAuthentication ? EAuthenticationType.Auto : EAuthenticationType.None);
            }
        }

        /// <summary>
        /// Gets the mail client state.
        /// </summary>
        public abstract MailClientState State
        {
            get;
        }

        /// <summary>
        /// Gets or sets the prioritised queued task processor mailbox.
        /// </summary>
        public Mailbox TaskProcessorMailbox
        {
            get
            {
                return TaskProcessor.ProcessingMailbox;
            }
            set
            {
                TaskProcessor.ProcessingMailbox = value;
            }
        }

        /// <summary>
        /// Gets the selected mailbox.
        /// </summary>
        public abstract Mailbox SelectedMailbox
        {
            get;
        }

        /// <summary>
        /// Gets the attachment directory.
        /// </summary>
        public abstract string AttachmentDirectory
        {
            get;
        }

        /// <summary>
        /// Gets the mailbox tree.
        /// </summary>
        public ObservableCollection<Mailbox> MailboxTree
        {
            get
            {
                return _mailboxTree;
            }
            protected set
            {
                if (_mailboxTree == value)
                    return;

                if (_mailboxTree == null
                 || !CollectionComparer.Compare<Mailbox>(_mailboxTree, value))
                {
                    _mailboxTree = value;
                    OnUpdatedMailboxTree(this, new MailboxTreeEventArgs(value));
                }
            }
        }

        /// <summary>
        /// Gets the mailbox restore state.
        /// </summary>
        public ConcurrentDictionary<string, MailboxActionState> MailboxRestoreStatus
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the mailbox download state.
        /// </summary>
        public ConcurrentDictionary<string, MailboxActionState> MailboxDownloadStatus
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the mailbox progress state.
        /// </summary>
        public ConcurrentDictionary<string, MessageProgressEventArgs> MailboxProgressStatus
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Inbox mailbox.
        /// </summary>
        public Mailbox Inbox
        {
            get
            {
                return MailboxTree.FirstOrDefault(o => o.Folder == MailboxFolders.Inbox);
            }
        }

        /// <summary>
        /// Gets the Drafts mailbox.
        /// </summary>
        public Mailbox Drafts
        {
            get
            {
                return MailboxTree.FirstOrDefault(o => o.Folder == MailboxFolders.Drafts);
            }
        }

        /// <summary>
        /// Gets the Outbox mailbox.
        /// </summary>
        public Mailbox Outbox
        {
            get
            {
                return MailboxTree.FirstOrDefault(o => o.Folder == MailboxFolders.Outbox);
            }
        }

        /// <summary>
        /// Gets the Sent Items mailbox.
        /// </summary>
        public Mailbox SentItems
        {
            get
            {
                return MailboxTree.FirstOrDefault(o => o.Folder == MailboxFolders.SentItems);
            }
        }

        /// <summary>
        /// Gets the Junk Mail mailbox.
        /// </summary>
        public Mailbox JunkMail
        {
            get
            {
                return MailboxTree.FirstOrDefault(o => o.Folder == MailboxFolders.JunkMail);
            }
        }

        /// <summary>
        /// Gets the Deleted Items mailbox.
        /// </summary>
        public Mailbox DeletedItems
        {
            get
            {
                return MailboxTree.FirstOrDefault(o => o.Folder == MailboxFolders.DeletedItems);
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Logs in to the mail account.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>true, if login is successfull; otherwise, false.</returns>
        protected abstract CommandResponseType LoginCommand(out string response);

        /// <summary>
        /// Logs out of the mail account.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType LogoutCommand(out string response);

        /// <summary>
        /// Gets the remote mailbox tree hierachy.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType GetMailboxTreeCommand(out string response);

        /// <summary>
        /// Gets the mailbox statistics.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="response">The response.</param>
        /// <param name="statistics">The statistics.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType StatisticsCommand(Mailbox mailbox, out string response, out IReadOnlyList<StatisticInfo> statistics);

        /// <summary>
        /// Downloads a message.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="statisticInfo">The statistic info.</param>
        /// <param name="response">The response.</param>
        /// <param name="message">The message.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType DownloadMessageCommand(Mailbox mailbox, StatisticInfo statisticInfo, out string response, out StructuredMessage message);

        /// <summary>
        /// Marks a message as read.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as undelete.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType MarkAsReadCommand(Mailbox mailbox, StructuredMessage message, out string response);

        /// <summary>
        /// Marks a message as unread.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as undelete.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType MarkAsUnreadCommand(Mailbox mailbox, StructuredMessage message, out string response);

        /// <summary>
        /// Marks a message as deleted.
        /// </summary>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        /// <param name="message">The message to mark as undelete.</param>
        /// <param name="response">The response.</param>
        /// <param name="newId">The new id.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType MarkAsDeletedCommand(Mailbox sourceMailbox, Mailbox destinationMailbox, StructuredMessage message, out string response, out string newId);

        /// <summary>
        /// Marks a message as undeleted.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as undelete.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType MarkAsUndeletedCommand(Mailbox mailbox, StructuredMessage message, out string response);

        /// <summary>
        /// Marks a message as flagged.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as flagged.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType MarkAsFlaggedCommand(Mailbox mailbox, StructuredMessage message, out string response);

        /// <summary>
        /// Marks a message as unflagged.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as unflagged.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType MarkAsUnflaggedCommand(Mailbox mailbox, StructuredMessage message, out string response);

        /// <summary>
        /// Moves a list of messages from it's current mailbox to a specified mailbox.
        /// </summary>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        /// <param name="messagePaths">The message paths to move.</param>
        /// <param name="response">The response.</param>
        /// <param name="newUids">The new uids.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType MoveMessageCommand(Mailbox sourceMailbox, Mailbox destinationMailbox, Dictionary<string, string> messagePaths, out string response, out Dictionary<string, string> newUids);

        /// <summary>
        /// Deletes the specified message.
        /// </summary>
        /// <param name="mailbox">The mailbox to delete from.</param>
        /// <param name="messagePaths">The message paths to delete.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType DeleteMessageCommand(Mailbox mailbox, Dictionary<string, string> messagePaths, out string response);

        /// <summary>
        /// Selects a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        protected abstract void SelectMailbox(Mailbox mailbox);

        /// <summary>
        /// Creates a new mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType AddMailboxCommand(Mailbox mailbox, out string response);

        /// <summary>
        /// Renames a mailbox.
        /// </summary>
        /// <param name="oldMailbox">The old mailbox.</param>
        /// <param name="renamedMailbox">The renamed mailbox.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType RenameMailboxCommand(Mailbox oldMailbox, Mailbox renamedMailbox, out string response);

        /// <summary>
        /// Deletes a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType RemoveMailboxCommand(Mailbox mailbox, out string response);

        /// <summary>
        /// Saves a message to drafts folder.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message.</param>
        /// <param name="response">The response.</param>
        /// <param name="uid">The uid.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType SaveToDraftsCommand(Mailbox mailbox, StructuredMessage message, out string response, out string uid);

        /// <summary>
        /// Gets the sent message uid.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="response">The response.</param>
        /// <param name="uid">The uid.</param>
        /// <returns>The response type.</returns>
        protected abstract CommandResponseType GetSentMessageUidCommand(Mailbox mailbox, string messageId, out string response, out string uid);

        #endregion

        #region Raise Event Methods

        /// <summary>
        /// Raises the Connected event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        protected void OnConnected(object sender, EventArgs e)
        {
            if (Connected != null)
                Connected(sender, e);
        }

        /// <summary>
        /// Raises the UpdatingMailboxTree event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxTreeEventArgs).</param>
        protected void OnUpdatingMailboxTree(object sender, MailboxTreeEventArgs e)
        {
            if (UpdatingMailboxTree != null)
                UpdatingMailboxTree(sender, e);
        }

        /// <summary>
        /// Raises the UpdatedMailboxTree event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxTreeEventArgs).</param>
        protected void OnUpdatedMailboxTree(object sender, MailboxTreeEventArgs e)
        {
            if (UpdatedMailboxTree != null)
                UpdatedMailboxTree(sender, e);
        }

        /// <summary>
        /// Raises the MailboxTreeUpdated event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxTreeFailedEventArgs).</param>
        protected void OnUpdateMailboxTreeFailed(object sender, MailboxTreeFailedEventArgs e)
        {
            if (UpdateMailboxTreeFailed != null)
                UpdateMailboxTreeFailed(sender, e);
        }

        /// <summary>
        /// Raises the LoggingIn event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        protected void OnLoggingIn(object sender, EventArgs e)
        {
            if (LoggingIn != null)
                LoggingIn(sender, e);
        }

        /// <summary>
        /// Raises the LoggedIn event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        protected void OnLoggedIn(object sender, EventArgs e)
        {
            if (LoggedIn != null)
                LoggedIn(sender, e);
        }

        /// <summary>
        /// Raises the LoginFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (FailedEventArgs).</param>
        protected void OnLoginFailed(object sender, FailedEventArgs e)
        {
            if (LoginFailed != null)
                LoginFailed(sender, e);
        }

        /// <summary>
        /// Raises the RestoreStarted event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnRestoreStarted(object sender, MailboxEventArgs e)
        {
            if (RestoreStarted != null)
                RestoreStarted(sender, e);
        }

        /// <summary>
        /// Raises the RestoringMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageProgressEventArgs).</param>
        protected void OnRestoringMessage(object sender, MessageProgressEventArgs e)
        {
            if (RestoringMessage != null)
                RestoringMessage(sender, e);
        }

        /// <summary>
        /// Raises the RestoredMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnRestoredMessage(object sender, MessageEventArgs e)
        {
            if (RestoredMessage != null)
                RestoredMessage(sender, e);
        }

        /// <summary>
        /// Raises the RestoreMessageFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        protected void OnRestoreMessageFailed(object sender, MessageFailedEventArgs e)
        {
            if (RestoreMessageFailed != null)
                RestoreMessageFailed(sender, e);
        }

        /// <summary>
        /// Raises the RestoreCompleted event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnRestoreCompleted(object sender, MailboxEventArgs e)
        {
            if (RestoreCompleted != null)
                RestoreCompleted(sender, e);
        }

        /// <summary>
        /// Raises the DownloadStarted event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnDownloadStarted(object sender, MailboxEventArgs e)
        {
            if (DownloadStarted != null)
                DownloadStarted(sender, e);
        }

        /// <summary>
        /// Raises the DownloadingMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (DownloadingMessageEventArgs).</param>
        protected void OnDownloadingMessage(object sender, MessageProgressEventArgs e)
        {
            if (DownloadingMessage != null)
                DownloadingMessage(sender, e);
        }

        /// <summary>
        /// Raises the DownloadedMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnDownloadedMessage(object sender, MessageEventArgs e)
        {
            if (DownloadedMessage != null)
                DownloadedMessage(sender, e);
        }

        /// <summary>
        /// Raises the DownloadMessageFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        protected void OnDownloadMessageFailed(object sender, MessageFailedEventArgs e)
        {
            if (DownloadMessageFailed != null)
                DownloadMessageFailed(sender, e);
        }

        /// <summary>
        /// Raises the DownloadCompleted event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        protected void OnDownloadCompleted(object sender, MailboxEventArgs e)
        {
            if (DownloadCompleted != null)
                DownloadCompleted(sender, e);
        }

        /// <summary>
        /// Raises the UpdatedMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnUpdatedMessage(object sender, MessageEventArgs e)
        {
            if (UpdatedMessage != null)
                UpdatedMessage(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as read.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkingMessageAsRead(object sender, MessageEventArgs e)
        {
            if (MarkingMessageAsRead != null)
                MarkingMessageAsRead(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as read attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsRead(object sender, MessageEventArgs e)
        {
            if (MarkMessageAsRead != null)
                MarkMessageAsRead(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as read attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsReadFailed(object sender, MessageFailedEventArgs e)
        {
            if (MarkMessageAsReadFailed != null)
                MarkMessageAsReadFailed(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as unread.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkingMessageAsUnread(object sender, MessageEventArgs e)
        {
            if (MarkingMessageAsUnread != null)
                MarkingMessageAsUnread(sender, e);
        }


        /// <summary>
        /// Occurs when a mail client mark a message as unread attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsUnread(object sender, MessageEventArgs e)
        {
            if (MarkMessageAsUnread != null)
                MarkMessageAsUnread(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as unread attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsUnreadFailed(object sender, MessageFailedEventArgs e)
        {
            if (MarkMessageAsUnreadFailed != null)
                MarkMessageAsUnreadFailed(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as deleted.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkingMessageAsDeleted(object sender, MessageEventArgs e)
        {
            if (MarkingMessageAsDeleted != null)
                MarkingMessageAsDeleted(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as deleted attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsDeleted(object sender, MessageEventArgs e)
        {
            if (MarkMessageAsDeleted != null)
                MarkMessageAsDeleted(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as deleted attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsDeletedFailed(object sender, MessageFailedEventArgs e)
        {
            if (MarkMessageAsDeletedFailed != null)
                MarkMessageAsDeletedFailed(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as undeleted.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkingMessageAsUndeleted(object sender, MessageEventArgs e)
        {
            if (MarkingMessageAsUndeleted != null)
                MarkingMessageAsUndeleted(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as undeleted attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsUndeleted(object sender, MessageEventArgs e)
        {
            if (MarkMessageAsUndeleted != null)
                MarkMessageAsUndeleted(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as undeleted attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsUndeletedFailed(object sender, MessageFailedEventArgs e)
        {
            if (MarkMessageAsUndeletedFailed != null)
                MarkMessageAsUndeletedFailed(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as flagged.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkingMessageAsFlagged(object sender, MessageEventArgs e)
        {
            if (MarkingMessageAsFlagged != null)
                MarkingMessageAsFlagged(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as flagged attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsFlagged(object sender, MessageEventArgs e)
        {
            if (MarkMessageAsFlagged != null)
                MarkMessageAsFlagged(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as flagged attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsFlaggedFailed(object sender, MessageFailedEventArgs e)
        {
            if (MarkMessageAsFlaggedFailed != null)
                MarkMessageAsFlaggedFailed(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client attempts to mark a message as unflagged.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkingMessageAsUnflagged(object sender, MessageEventArgs e)
        {
            if (MarkingMessageAsUnflagged != null)
                MarkingMessageAsUnflagged(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as unflagged attempt succeeds.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsUnflagged(object sender, MessageEventArgs e)
        {
            if (MarkMessageAsUnflagged != null)
                MarkMessageAsUnflagged(sender, e);
        }

        /// <summary>
        /// Occurs when a mail client mark a message as unflagged attempt fails.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnMarkedMessageAsUnflaggedFailed(object sender, MessageFailedEventArgs e)
        {
            if (MarkMessageAsUnflaggedFailed != null)
                MarkMessageAsUnflaggedFailed(sender, e);
        }

        /// <summary>
        /// Raises the MovingMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MoveMessageEventArgs).</param>
        protected void OnMovingMessage(object sender, MoveMessageEventArgs e)
        {
            if (MovingMessage != null)
                MovingMessage(sender, e);
        }

        /// <summary>
        /// Raises the MovedMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MoveMessageEventArgs).</param>
        protected void OnMovedMessage(object sender, MoveMessageEventArgs e)
        {
            if (MovedMessage != null)
                MovedMessage(sender, e);
        }

        /// <summary>
        /// Raises the MoveMessageFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        protected void OnMoveMessageFailed(object sender, MessageFailedEventArgs e)
        {
            if (MoveMessageFailed != null)
                MoveMessageFailed(sender, e);
        }

        /// <summary>
        /// Raises the DeletingMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnDeletingMessage(object sender, DeleteMessageEventArgs e)
        {
            if (DeletingMessage != null)
                DeletingMessage(sender, e);
        }

        /// <summary>
        /// Raises the DeletedMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageEventArgs).</param>
        protected void OnDeletedMessage(object sender, DeleteMessageEventArgs e)
        {
            if (DeletedMessage != null)
                DeletedMessage(sender, e);
        }

        /// <summary>
        /// Raises the DeleteMessageFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        protected void OnDeleteMessageFailed(object sender, DeleteMessageFailedEventArgs e)
        {
            if (DeleteMessageFailed != null)
                DeleteMessageFailed(sender, e);
        }

        /// <summary>
        /// Raises the AddingMailbox event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnAddingMailbox(object sender, MailboxEventArgs e)
        {
            if (AddingMailbox != null)
                AddingMailbox(sender, e);
        }

        /// <summary>
        /// Raises the AddedMailbox event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnAddedMailbox(object sender, MailboxEventArgs e)
        {
            if (AddedMailbox != null)
                AddedMailbox(sender, e);
        }

        /// <summary>
        /// Raises the AddMailboxFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        protected void OnAddMailboxFailed(object sender, MailboxFailedEventArgs e)
        {
            if (AddMailboxFailed != null)
                AddMailboxFailed(sender, e);
        }

        /// <summary>
        /// Raises the RenamingMailbox event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnRenamingMailbox(object sender, RenameMailboxEventArgs e)
        {
            if (RenamingMailbox != null)
                RenamingMailbox(sender, e);
        }

        /// <summary>
        /// Raises the RenamedMailbox event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnRenamedMailbox(object sender, RenameMailboxEventArgs e)
        {
            if (RenamedMailbox != null)
                RenamedMailbox(sender, e);
        }

        /// <summary>
        /// Raises the RenameMailboxFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        protected void OnRenameMailboxFailed(object sender, RenameMailboxFailedEventArgs e)
        {
            if (RenameMailboxFailed != null)
                RenameMailboxFailed(sender, e);
        }

        /// <summary>
        /// Raises the RemovingMailbox event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnRemovingMailbox(object sender, MailboxEventArgs e)
        {
            if (RemovingMailbox != null)
                RemovingMailbox(sender, e);
        }

        /// <summary>
        /// Raises the RemovedMailbox event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MailboxEventArgs).</param>
        protected void OnRemovedMailbox(object sender, MailboxEventArgs e)
        {
            if (RemovedMailbox != null)
                RemovedMailbox(sender, e);
        }

        /// <summary>
        /// Raises the RemoveMailboxFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (MessageFailedEventArgs).</param>
        protected void OnRemoveMailboxFailed(object sender, MailboxFailedEventArgs e)
        {
            if (RemoveMailboxFailed != null)
                RemoveMailboxFailed(sender, e);
        }

        /// <summary>
        /// Raises the SavingToDrafts event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SaveToDraftsEventArgs).</param>
        protected void OnSavingToDrafts(object sender, SaveToDraftsEventArgs e)
        {
            if (SavingToDrafts != null)
                SavingToDrafts(sender, e);
        }

        /// <summary>
        /// Raises the SavedToDrafts event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SaveToDraftsEventArgs).</param>
        protected void OnSavedToDrafts(object sender, SaveToDraftsEventArgs e)
        {
            if (SavedToDrafts != null)
                SavedToDrafts(sender, e);
        }

        /// <summary>
        /// Raises the SaveToDraftsFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SaveToDraftsFailedEventArgs).</param>
        protected void OnSaveToDraftsFailed(object sender, SaveToDraftsFailedEventArgs e)
        {
            if (SaveToDraftsFailed != null)
                SaveToDraftsFailed(sender, e);
        }

        /// <summary>
        /// Raises the SendingMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SendMessageEventArgs).</param>
        protected void OnSendingMessage(object sender, SendMessageEventArgs e)
        {
            if (SendingMessage != null)
                SendingMessage(sender, e);
        }

        /// <summary>
        /// Raises the SentMessage event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SendMessageEventArgs).</param>
        protected void OnSentMessage(object sender, SendMessageEventArgs e)
        {
            if (SentMessage != null)
                SentMessage(sender, e);
        }

        /// <summary>
        /// Raises the SendMessageFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (SendMessageFailedEventArgs).</param>
        protected void OnSendMessageFailed(object sender, SendMessageFailedEventArgs e)
        {
            if (SendMessageFailed != null)
                SendMessageFailed(sender, e);
        }

        /// <summary>
        /// Raises the LoggingOut event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        protected void OnLoggingOut(object sender, EventArgs e)
        {
            if (LoggingOut != null)
                LoggingOut(sender, e);
        }

        /// <summary>
        /// Raises the LoggedOut event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        protected void OnLoggedOut(object sender, EventArgs e)
        {
            if (LoggedOut != null)
                LoggedOut(sender, e);
        }

        /// <summary>
        /// Raises the LogoutFailed event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (FailedEventArgs).</param>
        protected void OnLogoutFailed(object sender, FailedEventArgs e)
        {
            if (LogoutFailed != null)
                LogoutFailed(sender, e);
        }

        /// <summary>
        /// Raises the Disconnected event.
        /// </summary>
        /// <param name="sender">The object that raised the event (MailClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        protected void OnDisconnected(object sender, EventArgs e)
        {
            if (Disconnected != null)
                Disconnected(sender, e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start the timer.
        /// </summary>
        private void StartTimer()
        {
            // Stop the timer
            StopTimer();

            // If mail must be synced and automatically
            if (AccountSettingsData.ContentToSyncEmail
             && AccountSettingsData.DownloadNewEmail != DownloadNewEmailOptions.Manual)
            {
                // Set the interval and start the timer
                backgroundTimer.Interval = new TimeSpan(0, (int)AccountSettingsData.DownloadNewEmail, 0);
                backgroundTimer.IsEnabled = true;
                backgroundTimer.Start();
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private void StopTimer()
        {
            // Stop the timer
            backgroundTimer.IsEnabled = false;
            backgroundTimer.Stop();
        }

        /// <summary>
        /// Occurs when a property on AccountSettingsData changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (AccountSettingsData).</param>
        /// <param name="e">The event data (PropertyChangedEventArgs).</param>
        private void AccountSettingsData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If DownloadNewEmail
            if (e.PropertyName == "DownloadNewEmail"
             || e.PropertyName == "ContentToSyncEmail")
            {
                // Stop the timer
                StopTimer();
                // Start timer - taking account into any changes
                StartTimer();
            }
        }

        /// <summary>
        /// Occurs when the background timer reaches it's interval.
        /// </summary>
        /// <param name="sender">The object that raised the event (BackgroundTimer).</param>
        /// <param name="e">The event data (Object).</param>
        private void backgroundTimer_Tick(object sender, object e)
        {
            try
            {
                if (IsInternetAvailable())
                    AutoSync();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(AccountSettingsData.AccountName, "", ex.ToString());
            }
        }

        /// <summary>
        /// Determines if the internet is available.
        /// </summary>
        /// <returns>true if internet is available; otherwise, false.</returns>
        public static bool IsInternetAvailable()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            return (connections != null) && (connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }

        /// <summary>
        /// Loads a message from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A message.</returns>
        public async Task<StructuredMessage> LoadMessage(string path)
        {
            return await IOUtil.LoadMessage(this is ImapMailClient, path);
        }

        /// <summary>
        /// Loads a message from the specified path.
        /// </summary>
        /// <param name="storageFile">The storage file.</param>
        /// <returns>A message.</returns>
        public async Task<StructuredMessage> LoadMessage(StorageFile storageFile)
        {
            return await IOUtil.LoadMessage(this is ImapMailClient, storageFile);
        }

        /// <summary>
        /// Determines whether this instance and another specified MailClient object have the same value.
        /// </summary>
        /// <param name="obj">The MailClient to compare to this instance.</param>
        /// <returns>true if the value of the value parameter is the same as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            MailClient mailClient = (MailClient)obj;
            return AccountSettingsData.EmailAddress.Equals(mailClient.AccountSettingsData.EmailAddress);
        }

        /// <summary>
        /// Returns the hash code for this MailClient.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return AccountSettingsData.EmailAddress.GetHashCode();
        }

        /// <summary>
        /// Gets the local mailbox tree hierachy.
        /// </summary>
        /// <returns>The local mailbox tree hierachy.</returns>
        public async Task GetLocalMailboxTree()
        {
            // Get local mailbox tree
            Mailbox mailbox = Mailbox.NewMailbox(AccountSettingsData.EmailAddress);
            List<Mailbox> mailboxTree = new List<Mailbox>();
            GetLocalMailboxTree(await IOUtil.GetCreateFolder(Path.Combine(ApplicationData.Current.LocalFolder.Path, AccountSettingsData.EmailAddress), FolderType.Account), mailboxTree, ref mailbox);

            if (this is PopMailClient)
            {
                // Add the standard mailboxes
                AddStandardMailbox(mailboxTree, ref mailbox, MailboxFolders.Inbox);
                AddStandardMailbox(mailboxTree, ref mailbox, MailboxFolders.Drafts);
                AddStandardMailbox(mailboxTree, ref mailbox, MailboxFolders.Outbox);
                AddStandardMailbox(mailboxTree, ref mailbox, MailboxFolders.SentItems);
                AddStandardMailbox(mailboxTree, ref mailbox, MailboxFolders.JunkMail);
                AddStandardMailbox(mailboxTree, ref mailbox, MailboxFolders.DeletedItems);
            }

            // Arrange mailbox tree
            MailboxTree = new ObservableCollection<Mailbox>(mailboxTree.OrderBy(o => (int)o.Folder).ThenBy(o => o.FullName));

            // Fill the dictionaries
            foreach (Mailbox mailboxTreeItem in MailboxTree)
                UpdateStatusDictionaries(mailboxTreeItem);
        }

        /// <summary>
        /// Gets the local mailbox tree hierachy.
        /// </summary>
        /// <param name="root">The root directory.</param>
        /// <param name="mailboxTree">The mailbox tree.</param>
        /// <param name="mailbox">The root mailbox.</param>
        /// <returns>The local mailbox tree hierachy.</returns>
        private void GetLocalMailboxTree(StorageFolder root, List<Mailbox> mailboxTree, ref Mailbox mailbox)
        {
            // Loop through each folder
            foreach (StorageFolder folder in Task.Run(async () => await root.GetFoldersAsync().AsTask().ConfigureAwait(false)).Result)
            {
                // Make sure it's a mailbox folder
                if (folder.Name[0] == MailClient.MailboxFolderPrefix)
                {
                    // Create the item
                    string name = null;
                    string displayName = null;
                    Task.Run(async () =>
                    {
                        StorageFile mailboxFile = await IOUtil.GetCreateFile(folder, MailboxFilename, CreationCollisionOption.OpenIfExists);
                        displayName = await FileIO.ReadTextAsync(mailboxFile);
                    }).Wait();

                    // Set the folder name
                    name = folder.Name.Substring(1, folder.Name.Length - 1);

                    // Set the display name
                    if (string.IsNullOrEmpty(displayName))
                        displayName = name;

                    // Create the mailbox
                    Mailbox newMailbox = Mailbox.NewMailbox(name, (mailbox.Name.Contains("@") ? null : mailbox), displayName);
                    mailbox.Children.Add(newMailbox);
                    mailboxTree.Add(newMailbox);

                    // Build the children
                    GetLocalMailboxTree(folder, mailboxTree, ref newMailbox);
                }
                else
                    continue;
            }
        }

        /// <summary>
        /// Adds the specified standard mailbox if not found.
        /// </summary>
        /// <param name="mailboxTree">The mailbox tree.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="folder">The folder.</param>
        protected void AddStandardMailbox(List<Mailbox> mailboxTree, ref Mailbox mailbox, MailboxFolders folder)
        {
            // Add if not found
            if (mailboxTree.FirstOrDefault(o => o.Folder == folder) == null)
            {
                Mailbox newMailbox = Mailbox.NewMailbox(folder.ToString().ToWords());
                mailboxTree.Add(newMailbox);
                UpdateStatusDictionaries(newMailbox);
            }
        }

        /// <summary>
        /// Updates the status dictionaries.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        protected void UpdateStatusDictionaries(Mailbox mailbox)
        {
            // Add missing mailbox restore statuses
            if (!MailboxRestoreStatus.ContainsKey(mailbox.FullName))
                MailboxRestoreStatus.TryAdd(mailbox.FullName, MailboxActionState.Completed);

            // Add missing mailbox download statuses
            if (!MailboxDownloadStatus.ContainsKey(mailbox.FullName))
            {
                // Stores the mailbox download status
                MailboxActionState mailboxDownloadStatus;

                // Determine which folder we are working with
                switch (mailbox.Folder)
                {
                    // Outbox is always completed
                    case MailboxFolders.Outbox:
                        mailboxDownloadStatus = MailboxActionState.Completed;
                        break;

                    // Any other folder
                    default:
                        // If Pop and not Inbox - always completed
                        if (this is PopMailClient
                         && mailbox.Folder != MailboxFolders.Inbox)
                            mailboxDownloadStatus = MailboxActionState.Completed;
                        // Else if Imap or Inbox - busy
                        else
                            mailboxDownloadStatus = MailboxActionState.Completed;
                        break;
                }
                MailboxDownloadStatus.TryAdd(mailbox.FullName, mailboxDownloadStatus);
            }

            // Add missing mailbox progress statuses
            if (!MailboxProgressStatus.ContainsKey(mailbox.FullName))
                MailboxProgressStatus.TryAdd(mailbox.FullName, new MessageProgressEventArgs(mailbox, new StatisticInfo("0", 0, 0, null), 0, 0, 0, 0, 0, 0));
        }

        /// <summary>
        /// Gets the email server provider ssl option.
        /// </summary>
        /// <param name="isOutgoingMailServerSsl">true if outgoing mail server uses ssl.</param>
        /// <param name="emailServiceProvider">The email service provider.</param>
        /// <returns>The smtp authentication interaction type.</returns>
        private static EInteractionType GetEmailServiceProviderSsl(bool isOutgoingMailServerSsl, EmailServiceProvider emailServiceProvider)
        {
            // If use ssl
            if (isOutgoingMailServerSsl)
            {
                // Determine the email service provider
                switch (emailServiceProvider)
                {
                    // TLS providers
                    case EmailServiceProvider.Aol:
                    case EmailServiceProvider.Hotmail:
                    case EmailServiceProvider.Outlook:
                        return EInteractionType.StartTLS;

                    // SSL providers
                    case EmailServiceProvider.Gmail:
                    case EmailServiceProvider.Yahoo:
                    case EmailServiceProvider.Gmx:
                    case EmailServiceProvider.Zoho:
                    default:
                        return EInteractionType.SSLPort;
                }
            }
            // Return plain
            return EInteractionType.Plain;
        }

        #endregion

        #region Common Methods

        /// <summary>
        /// Logs in to the mail account.
        /// </summary>
        /// <param name="loginOnly">true, if login only.</param>
        /// <returns>true, if login is successfull; otherwise, false.</returns>
        public AuthenticationResult Login(bool loginOnly = false)
        {
            try
            {
                // Try logging in
                OnLoggingIn(this, EventArgs.Empty);
                string response = null;
                CommandResponseType loginResponse = LoginCommand(out response);

                // If login successfull
                if (loginResponse == CommandResponseType.Ok)
                {
                    // Create the local folder structure upfront
                    GetMailboxTree();
                    if (!loginOnly) OnLoggedIn(this, EventArgs.Empty);
                    return new AuthenticationResult(true, string.Empty);
                }
                // Else if login failed
                else
                {
                    if (!loginOnly) OnLoginFailed(this, new FailedEventArgs(new Exception(response), string.Format("Login attempt for account '{0}' failed.", AccountSettingsData.AccountName)));
                    if (string.IsNullOrEmpty(response))
                        response = "UserName or Password is incorrect.";
                    if (!response.EndsWith("."))
                        response += ".";
                    return new AuthenticationResult(false, response);
                }
            }
            // If login failed
            catch (Exception ex)
            {
                if (!loginOnly) OnLoginFailed(this, new FailedEventArgs(ex, string.Format("Account '{0}': Login failed.", AccountSettingsData.AccountName)));
                return new AuthenticationResult(false, "An error occurred while trying to log into your account.");
            }
        }

        /// <summary>
        /// Logs out of the mail account.
        /// </summary>
        /// <returns>true, if logout is successfull; otherwise, false.</returns>
        public async Task<AuthenticationResult> Logout()
        {
            try
            {
                // Stop processing
                await TaskProcessor.Stop();
                await OutboxTaskProcessor.Stop();

                // Try logging out
                OnLoggingOut(this, EventArgs.Empty);
                string response = null;
                CommandResponseType logoutResponse = LogoutCommand(out response);

                // If logout successfull
                if (logoutResponse == CommandResponseType.Ok)
                {
                    OnLoggedOut(this, EventArgs.Empty);
                    return new AuthenticationResult(true, string.Empty);
                }
                // Else if logout failed
                else
                {
                    OnLogoutFailed(this, new FailedEventArgs(new Exception(response), string.Format("Account '{0}': Logout failed.", AccountSettingsData.AccountName)));
                    return new AuthenticationResult(false, response);
                }
            }
            // If login failed
            catch (Exception ex)
            {
                OnLogoutFailed(this, new FailedEventArgs(ex, string.Format("Account '{0}': Logout failed.", AccountSettingsData.AccountName)));
                return new AuthenticationResult(false, "Logout failed.");
            }
        }

        /// <summary>
        /// Gets the mailbox tree hierachy.
        /// </summary>
        /// <returns>The remote mailbox tree hierachy.</returns>
        public void GetMailboxTree()
        {
            try
            {
                // Try logging in
                OnUpdatingMailboxTree(this, null);
                string response = null;
                CommandResponseType getMailboxTreeResponse = GetMailboxTreeCommand(out response);

                // If login successfull
                if (getMailboxTreeResponse != CommandResponseType.Ok)
                {
                    OnUpdateMailboxTreeFailed(this, new MailboxTreeFailedEventArgs(MailboxTree, new Exception(response), string.Format("Get mailbox tree attempt for account '{0}' failed.{1}Error:{2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If login failed
            catch (Exception ex)
            {
                OnUpdateMailboxTreeFailed(this, new MailboxTreeFailedEventArgs(MailboxTree, ex, string.Format("Account '{0}': Get mailbox tree failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Restores all messages from disk.
        /// </summary>
        public void RestoreMessages()
        {
            // Loop through all the mailboxes
            foreach (Mailbox mailbox in MailboxTree)
            {
                // Start restoring this mailbox
                MailboxRestoreStatus[mailbox.FullName] = MailboxActionState.Busy;
                OnRestoreStarted(this, new MailboxEventArgs(mailbox));
                string uid = null;
                StructuredMessage message = null;

                try
                {
                    Task.Run(async () =>
                    {
                        // Set the mailbox
                        SelectMailbox(mailbox);

                        // Get the path of this folder
                        StorageFolder rootFolder = await IOUtil.GetCreateFolder(Path.Combine(ApplicationData.Current.LocalFolder.Path, AccountSettingsData.EmailAddress, mailbox.FullName), FolderType.Mailbox);

                        // Get all the folders in this path
                        IReadOnlyList<StorageFolder> messageFolders = await rootFolder.GetFoldersAsync();
                        uint totalMessageCount = (uint)messageFolders.Count;
                        uint totalMessageSize = 0;
                        uint currentMessageCount = 0;
                        uint currentMessageSize = 0;
                        uint remainingMessageCount = 0;
                        uint remainingMessageSize = 0;

                        // Loop through all the message folders
                        foreach (StorageFolder messageFolder in messageFolders)
                        {
                            try
                            {
                                // If it's a mailbox folder
                                if (messageFolder.Name.StartsWith(MailClient.MessageFolderPrefix.ToString()))
                                {
                                    // Set the counters
                                    uid = messageFolder.Name.Substring(1);
                                    currentMessageCount++;
                                    currentMessageSize = 0;
                                    remainingMessageCount = currentMessageCount;
                                    remainingMessageSize = 0;

                                    // Get the file
                                    string path = Path.Combine(messageFolder.Path, StructuredMessage.Filename);
                                    StorageFile storageFile = null;
                                    if ((storageFile = await IOUtil.FileExists(path)) != null)
                                    {
                                        // Restore the file
                                        BasicProperties properties = await storageFile.GetBasicPropertiesAsync();
                                        if (properties.Size > 0)
                                        {
                                            OnRestoringMessage(this, new MessageProgressEventArgs(null, null, mailbox, new StatisticInfo(uid, 0, currentMessageSize, null), totalMessageCount, totalMessageSize, currentMessageCount, currentMessageSize, remainingMessageCount, remainingMessageSize));
                                            message = await LoadMessage(storageFile);
                                            OnRestoredMessage(this, new MessageEventArgs(mailbox, uid, message));
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnRestoreMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(mailbox, uid, message), ex, string.Format("Account '{0}': Restore Message failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
                            }
                        }

                        // Set the mailbox progress state for this restored mailbox
                        MailboxProgressStatus[mailbox.FullName] = new MessageProgressEventArgs(mailbox, new StatisticInfo(0.ToString(), 0, 0, null), totalMessageCount, totalMessageSize, currentMessageCount, currentMessageSize, totalMessageCount, remainingMessageSize);
                        MailboxDownloadStatus[mailbox.FullName] = (totalMessageCount > 0 || mailbox.Folder == MailboxFolders.Outbox || (this is PopMailClient && mailbox.Folder != MailboxFolders.Inbox) ? MailboxActionState.Completed : MailboxActionState.Busy);
                    }).Wait();
                }
                catch (Exception ex)
                {
                    OnRestoreMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(mailbox, uid, message), ex, string.Format("Account '{0}': Restore Message failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
                }
                finally
                {
                    // This mailbox is restored
                    MailboxRestoreStatus[mailbox.FullName] = MailboxActionState.Completed;
                    OnRestoreCompleted(this, new MailboxEventArgs(mailbox));
                }
            }

            // All mailboxes restore completed
            OnRestoreCompleted(this, new MailboxEventArgs(null));
        }

        /// <summary>
        /// Downloads all the unread messages.
        /// </summary>
        /// <param name="downloadMailbox">The download mailbox.</param>
        public async Task DownloadUnreadMessages(Mailbox downloadMailbox = null)
        {
            Mailbox currentMailbox = null;
            StructuredMessage message = null;
            string uid = null;
            try
            {
                // Stop processing
                await TaskProcessor.Stop();

                // Stores the mailboxes
                List<Mailbox> mailboxes = new List<Mailbox>();

                // If PopMailClient - inbox
                if (this is PopMailClient)
                    mailboxes.AddRange(MailboxTree.Where(o => o.Folder == MailboxFolders.Inbox));
                // Else if ImapMailClient - all unreserved
                else
                    mailboxes.AddRange(MailboxTree.Where(o => o.Folder != MailboxFolders.Outbox && !o.IsSystem));

                // Loop through each folder
                TaskProcessorMailbox = downloadMailbox;
                foreach (Mailbox mailbox in mailboxes)
                {
                    try
                    {
                        // Get the statistics
                        currentMailbox = mailbox;
                        SelectMailbox(mailbox);
                        string response;
                        IReadOnlyList<StatisticInfo> statisticsInfo = new List<StatisticInfo>();
                        if (StatisticsCommand(mailbox, out response, out statisticsInfo) == CommandResponseType.Ok)
                        {
                            uint totalMessageCount = (uint)statisticsInfo.Count;
                            uint totalMessageSize = (uint)statisticsInfo.Sum(o => o.MessageSize);
                            uint currentMessageSize = 0;
                            uint currentMessageCount = 0;
                            uint remainingMessageCount = totalMessageCount;
                            uint remainingMessageSize = totalMessageSize;

                            // Get the uids
                            StorageFile uidsFile = await IOUtil.GetCreateFile(await IOUtil.GetCreateFolder(AttachmentDirectory, FolderType.Mailbox), UidsFilename, CreationCollisionOption.OpenIfExists);
                            IList<string> storedUids = await FileIO.ReadLinesAsync(uidsFile);

                            // Indicate that the download has started for this mailbox
                            OnDownloadStarted(this, new MailboxEventArgs(mailbox));

                            // Sync messages
                            await SyncMessages(storedUids, statisticsInfo, mailbox);

                            // Loop through each message
                            for (int i = 0; i < (int)totalMessageCount; i++)
                            {
                                // Get the message size
                                StatisticInfo info = statisticsInfo[i];
                                uid = info.UniqueNumber;
                                currentMessageCount++;
                                currentMessageSize = info.MessageSize;
                                remainingMessageCount -= currentMessageCount;
                                remainingMessageSize -= currentMessageSize;

                                // Sync message flags
                                await SyncMessageFlags(storedUids, uid, info, mailbox);

                                // If the message is not in the queue - queue it
                                DownloadMessagePrioritisedQueuedTask downloadMessagePrioritisedQueuedTask = new DownloadMessagePrioritisedQueuedTask(new MessageProgressEventArgs(uidsFile, storedUids, mailbox, info, totalMessageCount, totalMessageSize, currentMessageCount, currentMessageSize, remainingMessageCount, remainingMessageSize));
                                TaskProcessor.Queue.Enqueue(downloadMessagePrioritisedQueuedTask);
                            }
                        }

                        // Set the mailbox progress state for this downloaded mailbox
                        if (MailboxDownloadStatus[mailbox.FullName] == MailboxActionState.Busy)
                            MailboxDownloadStatus[mailbox.FullName] = (statisticsInfo.Count == 0 ? MailboxActionState.Completed : MailboxActionState.Busy);
                    }
                    catch (Exception ex)
                    {
                        OnDownloadMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(mailbox, uid, message), ex, string.Format("Account '{0}': Message download failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
                    }
                }

                // Start processing
                await TaskProcessor.Start();
            }
            // If download failed
            catch (Exception ex)
            {
                OnDownloadMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(currentMailbox, uid, message), ex, string.Format("Account '{0}': Message download failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
            finally
            {
                // If Sync email and NOT manual downloads
                StartTimer();
            }
        }

        /// <summary>
        /// Downloads all the unread messages.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task DownloadUnreadMessagesTask(CancellationToken cancellationToken)
        {
            Mailbox currentMailbox = null;
            StructuredMessage message = null;
            string uid = null;
            try
            {
                // Stores the mailboxes
                List<Mailbox> mailboxes = new List<Mailbox>();

                // If PopMailClient - inbox
                if (this is PopMailClient)
                    mailboxes.AddRange(MailboxTree.Where(o => o.Folder == MailboxFolders.Inbox));
                // Else if ImapMailClient
                else
                {
                    // Get all the mailboxes
                    mailboxes.AddRange(MailboxTree
                                      .Where(o => !o.IsSystem && o.Folder != MailboxFolders.Outbox)
                                      .OrderBy(o => (int)o.Folder)
                                      .ThenBy(o => o.FullName));
                }

                // Loop through each folder
                foreach (Mailbox mailbox in mailboxes)
                {
                    try
                    {
                        // Check if we need to cancel
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        // Get the statistics
                        currentMailbox = mailbox;
                        SelectMailbox(mailbox);
                        string response;
                        IReadOnlyList<StatisticInfo> statisticsInfo;
                        if (StatisticsCommand(mailbox, out response, out statisticsInfo) == CommandResponseType.Ok)
                        {
                            // Check if we need to cancel
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            uint totalMessageCount = (uint)statisticsInfo.Count();
                            uint totalMessageSize = (uint)statisticsInfo.Sum(o => o.MessageSize);
                            uint currentMessageSize = 0;
                            uint currentMessageCount = 0;
                            uint remainingMessageCount = totalMessageCount;
                            uint remainingMessageSize = totalMessageSize;

                            // Get the uids
                            StorageFile uidsFile = await IOUtil.GetCreateFile(await IOUtil.GetCreateFolder(AttachmentDirectory, FolderType.Mailbox), UidsFilename, CreationCollisionOption.OpenIfExists);
                            IList<string> storedUids = await FileIO.ReadLinesAsync(uidsFile);

                            // Sync messages
                            await SyncMessages(storedUids, statisticsInfo, mailbox);

                            // Loop through each message
                            for (int i = 0; i < (int)totalMessageCount; i++)
                            {
                                // Check if we need to cancel
                                if (cancellationToken.IsCancellationRequested)
                                    return;

                                // Get the message size
                                StatisticInfo info = statisticsInfo[i];
                                uid = info.UniqueNumber;
                                currentMessageCount++;
                                currentMessageSize = info.MessageSize;
                                remainingMessageCount -= currentMessageCount;
                                remainingMessageSize -= currentMessageSize;

                                // Sync message flags
                                await SyncMessageFlags(storedUids, uid, info, mailbox);

                                // If the message is not in the queue - queue it
                                DownloadMessagePrioritisedQueuedTask downloadMessagePrioritisedQueuedTask = new DownloadMessagePrioritisedQueuedTask(new MessageProgressEventArgs(uidsFile, storedUids, mailbox, info, totalMessageCount, totalMessageSize, currentMessageCount, currentMessageSize, remainingMessageCount, remainingMessageSize));
                                await ProcessDownloadMessagePrioritisedQueuedTask(downloadMessagePrioritisedQueuedTask);
                            }
                        }
                        else
                            OnDownloadMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(mailbox, uid, message), new Exception(response), string.Format("Account '{0}': Message download failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                    }
                    catch (Exception ex)
                    {
                        OnDownloadMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(mailbox, uid, message), ex, string.Format("Account '{0}': Message download failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
                    }
                }
            }
            // If download failed
            catch (Exception ex)
            {
                OnDownloadMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(currentMailbox, uid, message), ex, string.Format("Account '{0}': Message download failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Sends unsent messages.
        /// </summary>
        public async Task SendUnsentMessages()
        {
            try
            {
                // Stop processing
                await OutboxTaskProcessor.Stop();

                // If there are any unsent messages
                StorageSettings.MailHeaderDictionary.EnsureMailHeader(AccountSettingsData, Outbox);
                if (OutboxTaskProcessor.Queue.GetQueuedTaskCount(PrioritisedQueuedTaskType.DownloadMessage) == 0
                 && StorageSettings.MailHeaderDictionary[AccountSettingsData.EmailAddress][Outbox.FullName].Count > 0)
                {
                    // Put all the unsent messages in the queue for processing
                    foreach (MailHeader mailHeader in StorageSettings.MailHeaderDictionary[AccountSettingsData.EmailAddress][Outbox.FullName])
                        OutboxTaskProcessor.Queue.Enqueue(await LoadMessage(mailHeader.MessagePath));
                }

                // Start processing
                await OutboxTaskProcessor.Start();
            }
            catch (Exception ex)
            {
                LogFile.Instance.LogError(AccountSettingsData.AccountName, (Outbox == null ? "Outbox" : Outbox.FullName), ex.ToString());
            }
        }

        /// <summary>
        /// Syncs messages.
        /// </summary>
        /// <param name="storedUids">The stored uids.</param>
        /// <param name="statisticsInfo">The statistics info.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <returns>A task.</returns>
        private async Task SyncMessages(IList<string> storedUids, IReadOnlyList<StatisticInfo> statisticsInfo, Mailbox mailbox)
        {
            // If Imap
            if (this is ImapMailClient)
            {
                // Get the server uids
                IEnumerable<string> serverUids = statisticsInfo.Select(o => o.UniqueNumber);

                // Get the deleted uids
                IEnumerable<string> deletedUids = (from storedUid in storedUids
                                                   where (!serverUids.Contains(storedUid))
                                                   select storedUid);

                // Delete messages that were deleted on the server
                foreach (string deletedUid in deletedUids)
                {
                    Dictionary<string, string> messagePath = new Dictionary<string, string>();
                    messagePath.Add(deletedUid, IOUtil.GetMessageFullPath(AccountSettingsData.EmailAddress, mailbox.FullName, deletedUid));
                    OnDeletingMessage(this, new DeleteMessageEventArgs(messagePath, mailbox));
                    await ProcessDeleteMessagePrioritisedQueuedTask(new DeleteMessagePrioritisedQueuedTask(new DeleteMessageEventArgs(messagePath, mailbox)));
                }
            }
        }

        /// <summary>
        /// Syncs the message flags.
        /// </summary>
        /// <param name="storedUids">The stored uids.</param>
        /// <param name="uid">The uid.</param>
        /// <param name="info">The statistic info.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <returns>A task.</returns>
        private async Task SyncMessageFlags(IList<string> storedUids, string uid, StatisticInfo info, Mailbox mailbox)
        {
            // If Imap
            if (this is ImapMailClient && storedUids.Contains(uid, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    // Get the message
                    string path = IOUtil.GetMessageFullPath(AccountSettingsData.EmailAddress, SelectedMailbox.FullName, uid);
                    StorageFile messageFile;
                    if ((messageFile = await IOUtil.FileExists(path)) != null)
                    {
                        // Check if the flags have changed
                        StructuredMessage message = await LoadMessage(messageFile);
                        if (message.IsDeleted != info.Flags.Contains(EFlag.Deleted)
                         || message.IsFlagged != info.Flags.Contains(EFlag.Flagged)
                         || message.IsSeen != info.Flags.Contains(EFlag.Seen))
                        {
                            // Update the flags if changed
                            message.IsDeleted = info.Flags.Contains(EFlag.Deleted);
                            message.IsFlagged = info.Flags.Contains(EFlag.Flagged);
                            message.IsSeen = info.Flags.Contains(EFlag.Seen);
                            await message.Save();
                            OnUpdatedMessage(this, new MessageEventArgs(mailbox, uid, message));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(AccountSettingsData.EmailAddress, mailbox.FullName, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Processes the download message prioritised queued task. 
        /// </summary>
        /// <param name="downloadMessagePrioritisedQueuedTask">The download message prioritised queued task.</param>
        internal async Task ProcessDownloadMessagePrioritisedQueuedTask(DownloadMessagePrioritisedQueuedTask downloadMessagePrioritisedQueuedTask)
        {
            MailboxProgressStatus[downloadMessagePrioritisedQueuedTask.MessageProgress.Mailbox.FullName] = downloadMessagePrioritisedQueuedTask.MessageProgress;
            OnDownloadingMessage(this, downloadMessagePrioritisedQueuedTask.MessageProgress);
            string response = null;
            StructuredMessage message = null;
            string uid = downloadMessagePrioritisedQueuedTask.MessageProgress.Uid;

            try
            {
                // If the message does exist
                if (!downloadMessagePrioritisedQueuedTask.MessageProgress.StoredUids.Contains(uid, StringComparer.OrdinalIgnoreCase))
                {
                    // Try to download the message
                    message = null;
                    SelectMailbox(downloadMessagePrioritisedQueuedTask.Mailbox);
                    if (DownloadMessageCommand(downloadMessagePrioritisedQueuedTask.Mailbox, downloadMessagePrioritisedQueuedTask.MessageProgress.StatisticInfo, out response, out message) == CommandResponseType.Ok)
                    {
                        downloadMessagePrioritisedQueuedTask.MessageProgress.StoredUids.Add(uid);
                        await message.Save(AccountSettingsData.EmailAddress, MailClient.MailboxFolderPrefix + SelectedMailbox.FullName, MailClient.MessageFolderPrefix + uid, false);
                        OnDownloadedMessage(this, new MessageEventArgs(downloadMessagePrioritisedQueuedTask.Mailbox, uid, message));
                    }
                    else
                        OnDownloadMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(downloadMessagePrioritisedQueuedTask.Mailbox, uid, message), new Exception(response), string.Format("Account '{0}': Message download failed - '{1}'.{2}Error: {3}", AccountSettingsData.AccountName, Environment.NewLine, uid, response)));

                    await FileIO.AppendTextAsync(downloadMessagePrioritisedQueuedTask.MessageProgress.UidsFile, uid + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // On error
                OnDownloadMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(downloadMessagePrioritisedQueuedTask.Mailbox, uid, message), ex, string.Format("Account '{0}': Message download failed - '{1}'.{2}Error: {3}", AccountSettingsData.AccountName, Environment.NewLine, uid, ex.Message)));
            }

            // Set the mailbox progress state for this downloaded mailbox
            if (MailboxDownloadStatus[downloadMessagePrioritisedQueuedTask.Mailbox.FullName] == MailboxActionState.Busy)
                MailboxDownloadStatus[downloadMessagePrioritisedQueuedTask.Mailbox.FullName] = MailboxActionState.Completed;
        }

        /// <summary>
        /// Marks a message as read.
        /// </summary>
        /// <param name="message">The message to mark as read.</param>
        /// <param name="mailbox">The mailbox.</param>
        public void MarkAsRead(StructuredMessage message, Mailbox mailbox)
        {
            OnMarkingMessageAsRead(this, new MessageEventArgs(mailbox, message.Uid, message));
            TaskProcessor.Queue.Enqueue(new MarkAsReadPrioritisedQueuedTask(message, mailbox));
        }

        /// <summary>
        /// Processes the mark as read prioritised queued task.
        /// </summary>
        /// <param name="markAsReadPrioritisedQueuedTask">The mark as read prioritised queued task.</param>
        internal async Task ProcessMarkAsReadPrioritisedQueuedTask(MarkAsReadPrioritisedQueuedTask markAsReadPrioritisedQueuedTask)
        {
            // Store the current IsRead value
            bool isSeen = markAsReadPrioritisedQueuedTask.Message.IsSeen;
            string response = null;
            try
            {
                // If marking successfull
                if (MarkAsReadCommand(markAsReadPrioritisedQueuedTask.Mailbox, markAsReadPrioritisedQueuedTask.Message, out response) == CommandResponseType.Ok)
                {
                    markAsReadPrioritisedQueuedTask.Message.IsSeen = true;
                    await markAsReadPrioritisedQueuedTask.Message.Save();
                    OnMarkedMessageAsRead(this, new MessageEventArgs(markAsReadPrioritisedQueuedTask.Mailbox, markAsReadPrioritisedQueuedTask.Uid, markAsReadPrioritisedQueuedTask.Message));
                }
                // Else if marking failed
                else
                {
                    markAsReadPrioritisedQueuedTask.Message.IsSeen = isSeen;
                    OnMarkedMessageAsReadFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsReadPrioritisedQueuedTask.Mailbox, markAsReadPrioritisedQueuedTask.Uid, markAsReadPrioritisedQueuedTask.Message), new Exception(response), string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If marking failed
            catch (Exception ex)
            {
                markAsReadPrioritisedQueuedTask.Message.IsSeen = isSeen;
                OnMarkedMessageAsReadFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsReadPrioritisedQueuedTask.Mailbox, markAsReadPrioritisedQueuedTask.Uid, markAsReadPrioritisedQueuedTask.Message), ex, string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Marks a message as unread.
        /// </summary>
        /// <param name="message">The message to mark as unread.</param>
        /// <param name="mailbox">The mailbox.</param>
        public void MarkAsUnread(StructuredMessage message, Mailbox mailbox)
        {
            OnMarkingMessageAsUnread(this, new MessageEventArgs(mailbox, message.Uid, message));
            TaskProcessor.Queue.Enqueue(new MarkAsUnreadPrioritisedQueuedTask(message, mailbox));
        }

        /// <summary>
        /// Processes the mark as unread prioritised queued task.
        /// </summary>
        /// <param name="markAsUnreadPrioritisedQueuedTask">The mark as unread prioritised queued task.</param>
        internal async Task ProcessMarkAsUnreadPrioritisedQueuedTask(MarkAsUnreadPrioritisedQueuedTask markAsUnreadPrioritisedQueuedTask)
        {
            // Store the current IsRead value
            bool isRead = markAsUnreadPrioritisedQueuedTask.Message.IsSeen;
            string response = null;

            try
            {
                // If marking successfull
                if (MarkAsUnreadCommand(markAsUnreadPrioritisedQueuedTask.Mailbox, markAsUnreadPrioritisedQueuedTask.Message, out response) == CommandResponseType.Ok)
                {
                    markAsUnreadPrioritisedQueuedTask.Message.IsSeen = false;
                    await markAsUnreadPrioritisedQueuedTask.Message.Save();
                    OnMarkedMessageAsUnread(this, new MessageEventArgs(markAsUnreadPrioritisedQueuedTask.Mailbox, markAsUnreadPrioritisedQueuedTask.Uid, markAsUnreadPrioritisedQueuedTask.Message));
                }
                // Else if marking failed
                else
                {
                    markAsUnreadPrioritisedQueuedTask.Message.IsSeen = isRead;
                    OnMarkedMessageAsUnreadFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsUnreadPrioritisedQueuedTask.Mailbox, markAsUnreadPrioritisedQueuedTask.Uid, markAsUnreadPrioritisedQueuedTask.Message), new Exception(response), string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If marking failed
            catch (Exception ex)
            {
                markAsUnreadPrioritisedQueuedTask.Message.IsSeen = isRead;
                OnMarkedMessageAsUnreadFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsUnreadPrioritisedQueuedTask.Mailbox, markAsUnreadPrioritisedQueuedTask.Uid, markAsUnreadPrioritisedQueuedTask.Message), ex, string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Marks a message as deleted.
        /// </summary>
        /// <param name="message">The message to mark as deleted.</param>
        /// <param name="mailbox">The mailbox.</param>
        public void MarkAsDeleted(StructuredMessage message, Mailbox mailbox)
        {
            OnMarkingMessageAsDeleted(this, new MessageEventArgs(mailbox, message.Uid, message));
            TaskProcessor.Queue.Enqueue(new MarkAsDeletedPrioritisedQueuedTask(message, mailbox, DeletedItems));
        }

        /// <summary>
        /// Processes the mark as deleted prioritised queued task.
        /// </summary>
        /// <param name="markAsDeletedPrioritisedQueuedTask">The mark as deleted prioritised queued task.</param>
        internal async Task ProcessMarkAsDeletedPrioritisedQueuedTask(MarkAsDeletedPrioritisedQueuedTask markAsDeletedPrioritisedQueuedTask)
        {
            // Store the current IsRead value
            bool isDeleted = markAsDeletedPrioritisedQueuedTask.Message.IsDeleted;
            string response = null;
            string newId = null;

            try
            {
                // If marking successfull
                if (MarkAsDeletedCommand(markAsDeletedPrioritisedQueuedTask.Mailbox, markAsDeletedPrioritisedQueuedTask.DestinationMailbox, markAsDeletedPrioritisedQueuedTask.Message, out response, out newId) == CommandResponseType.Ok)
                {
                    markAsDeletedPrioritisedQueuedTask.Message.IsDeleted = true;
                    await markAsDeletedPrioritisedQueuedTask.Message.Save();
                    Dictionary<string, string> messagePaths = new Dictionary<string, string>();
                    messagePaths.Add(markAsDeletedPrioritisedQueuedTask.Message.Uid, markAsDeletedPrioritisedQueuedTask.Message.MessagePath);
                    Dictionary<string, string> newIds = new Dictionary<string, string>();
                    newIds.Add(markAsDeletedPrioritisedQueuedTask.Message.Uid, newId);
                    Dictionary<string, StructuredMessage> messages = new Dictionary<string, StructuredMessage>();
                    await MoveMessageIO(messagePaths, newIds, messages, markAsDeletedPrioritisedQueuedTask.Mailbox, markAsDeletedPrioritisedQueuedTask.DestinationMailbox);
                    OnMarkedMessageAsDeleted(this, new MessageEventArgs(markAsDeletedPrioritisedQueuedTask.Mailbox, markAsDeletedPrioritisedQueuedTask.Uid, messages.First().Value));
                }
                // Else if marking failed
                else
                {
                    markAsDeletedPrioritisedQueuedTask.Message.IsDeleted = isDeleted;
                    OnMarkedMessageAsDeletedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsDeletedPrioritisedQueuedTask.Mailbox, markAsDeletedPrioritisedQueuedTask.Uid, markAsDeletedPrioritisedQueuedTask.Message), new Exception(response), string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If marking failed
            catch (Exception ex)
            {
                markAsDeletedPrioritisedQueuedTask.Message.IsDeleted = isDeleted;
                OnMarkedMessageAsDeletedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsDeletedPrioritisedQueuedTask.Mailbox, markAsDeletedPrioritisedQueuedTask.Uid, markAsDeletedPrioritisedQueuedTask.Message), ex, string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Deletes the specified message.
        /// </summary>
        /// <param name="messagePaths">The message paths to delete.</param>
        /// <param name="mailbox">The mailbox to delete from.</param>
        /// <param name="raiseOnDeletingMessage">true, if the DeletingMessage event should be raised; otherwise, false.</param>
        public void DeleteMessage(Dictionary<string, string> messagePaths, Mailbox mailbox, bool raiseOnDeletingMessage = true)
        {
            if (raiseOnDeletingMessage) OnDeletingMessage(this, new DeleteMessageEventArgs(messagePaths, mailbox));
            TaskProcessor.Queue.Enqueue(new DeleteMessagePrioritisedQueuedTask(new DeleteMessageEventArgs(messagePaths, mailbox)));
        }

        /// <summary>
        /// Processes the delete message prioritised queued task.
        /// </summary>
        /// <param name="deleteMessagePrioritisedQueuedTask">The delete message prioritised queued task.</param>
        internal async Task ProcessDeleteMessagePrioritisedQueuedTask(DeleteMessagePrioritisedQueuedTask deleteMessagePrioritisedQueuedTask)
        {
            try
            {
                string response = null;
                Dictionary<string, StructuredMessage> messages = new Dictionary<string, StructuredMessage>();

                // If delete successfull
                if (DeleteMessageCommand(deleteMessagePrioritisedQueuedTask.DeleteMessage.Mailbox, deleteMessagePrioritisedQueuedTask.DeleteMessage.MessagePaths, out response) == CommandResponseType.Ok)
                {
                    foreach (KeyValuePair<string, string> messagePath in deleteMessagePrioritisedQueuedTask.DeleteMessage.MessagePaths)
                    {
                        StorageFolder messageFolder = await StorageFolder.GetFolderFromPathAsync(messagePath.Value.Replace(MailMessage.Filename, ""));
                        await messageFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        await RemoveUid(messageFolder, messagePath.Key);
                        MailboxProgressStatus[deleteMessagePrioritisedQueuedTask.Mailbox.FullName].TotalMessageCount = MailboxProgressStatus[deleteMessagePrioritisedQueuedTask.Mailbox.FullName].TotalMessageCount - 1;
                        OnDownloadingMessage(this, MailboxProgressStatus[deleteMessagePrioritisedQueuedTask.Mailbox.FullName]);
                    }
                    OnDeletedMessage(this, deleteMessagePrioritisedQueuedTask.DeleteMessage);
                }
                // Else if marking failed
                else
                {
                    OnDeleteMessageFailed(this, new DeleteMessageFailedEventArgs(deleteMessagePrioritisedQueuedTask.DeleteMessage, new Exception(response), string.Format("Account '{0}': Delete message failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If marking failed
            catch (Exception ex)
            {
                OnDeleteMessageFailed(this, new DeleteMessageFailedEventArgs(deleteMessagePrioritisedQueuedTask.DeleteMessage, ex, string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Marks a message as undeleted.
        /// </summary>
        /// <param name="message">The message to mark as undeleted.</param>
        /// <param name="mailbox">The mailbox.</param>
        public void MarkAsUndeleted(StructuredMessage message, Mailbox mailbox)
        {
            OnMarkingMessageAsUndeleted(this, new MessageEventArgs(mailbox, message.Uid, message));
            TaskProcessor.Queue.Enqueue(new MarkAsUndeletedPrioritisedQueuedTask(message, mailbox));
        }

        /// <summary>
        /// Processes the mark as undeleted prioritised queued task.
        /// </summary>
        /// <param name="markAsUndeletedPrioritisedQueuedTask">The mark as undeleted prioritised queued task.</param>
        internal async Task ProcessMarkAsUndeletedPrioritisedQueuedTask(MarkAsUndeletedPrioritisedQueuedTask markAsUndeletedPrioritisedQueuedTask)
        {
            // Store the current IsRead value
            bool isDeleted = markAsUndeletedPrioritisedQueuedTask.Message.IsDeleted;
            string response = null;

            try
            {
                // If marking successfull
                if (MarkAsUndeletedCommand(markAsUndeletedPrioritisedQueuedTask.Mailbox, markAsUndeletedPrioritisedQueuedTask.Message, out response) == CommandResponseType.Ok)
                {
                    markAsUndeletedPrioritisedQueuedTask.Message.IsDeleted = false;
                    await markAsUndeletedPrioritisedQueuedTask.Message.Save();
                    OnMarkedMessageAsUndeleted(this, new MessageEventArgs(markAsUndeletedPrioritisedQueuedTask.Mailbox, markAsUndeletedPrioritisedQueuedTask.Uid, markAsUndeletedPrioritisedQueuedTask.Message));

                    Dictionary<string, string> messagePaths = new Dictionary<string, string>();
                    messagePaths.Add(markAsUndeletedPrioritisedQueuedTask.Message.Uid, markAsUndeletedPrioritisedQueuedTask.Message.MessagePath);
                    MoveMessage(messagePaths, DeletedItems, markAsUndeletedPrioritisedQueuedTask.Mailbox);
                }
                // Else if marking failed
                else
                {
                    markAsUndeletedPrioritisedQueuedTask.Message.IsDeleted = isDeleted;
                    OnMarkedMessageAsUndeletedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsUndeletedPrioritisedQueuedTask.Mailbox, markAsUndeletedPrioritisedQueuedTask.Uid, markAsUndeletedPrioritisedQueuedTask.Message), new Exception(response), string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If marking failed
            catch (Exception ex)
            {
                markAsUndeletedPrioritisedQueuedTask.Message.IsDeleted = isDeleted;
                OnMarkedMessageAsUndeletedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsUndeletedPrioritisedQueuedTask.Mailbox, markAsUndeletedPrioritisedQueuedTask.Uid, markAsUndeletedPrioritisedQueuedTask.Message), ex, string.Format("Account '{0}': Mark message as read failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Marks a message as flagged.
        /// </summary>
        /// <param name="message">The message to mark as flagged.</param>
        /// <param name="mailbox">The mailbox.</param>
        public void MarkAsFlagged(StructuredMessage message, Mailbox mailbox)
        {
            OnMarkingMessageAsFlagged(this, new MessageEventArgs(mailbox, message.Uid, message));
            TaskProcessor.Queue.Enqueue(new MarkAsFlaggedPrioritisedQueuedTask(message, mailbox));
        }

        /// <summary>
        /// Processes the mark as flagged prioritised queued task.
        /// </summary>
        /// <param name="markAsFlaggedPrioritisedQueuedTask">The mark as flagged prioritised queued task.</param>
        internal async Task ProcessMarkAsFlaggedPrioritisedQueuedTask(MarkAsFlaggedPrioritisedQueuedTask markAsFlaggedPrioritisedQueuedTask)
        {
            // Store the current IsFlagged value
            bool isFlagged = markAsFlaggedPrioritisedQueuedTask.Message.IsFlagged;
            string response = null;
            try
            {
                // If marking successfull
                if (MarkAsFlaggedCommand(markAsFlaggedPrioritisedQueuedTask.Mailbox, markAsFlaggedPrioritisedQueuedTask.Message, out response) == CommandResponseType.Ok)
                {
                    markAsFlaggedPrioritisedQueuedTask.Message.IsFlagged = true;
                    await markAsFlaggedPrioritisedQueuedTask.Message.Save();
                    OnMarkedMessageAsFlagged(this, new MessageEventArgs(markAsFlaggedPrioritisedQueuedTask.Mailbox, markAsFlaggedPrioritisedQueuedTask.Uid, markAsFlaggedPrioritisedQueuedTask.Message));
                }
                // Else if marking failed
                else
                {
                    markAsFlaggedPrioritisedQueuedTask.Message.IsFlagged = isFlagged;
                    OnMarkedMessageAsFlaggedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsFlaggedPrioritisedQueuedTask.Mailbox, markAsFlaggedPrioritisedQueuedTask.Uid, markAsFlaggedPrioritisedQueuedTask.Message), new Exception(response), string.Format("Account '{0}': Mark message as flagged failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If marking failed
            catch (Exception ex)
            {
                markAsFlaggedPrioritisedQueuedTask.Message.IsFlagged = isFlagged;
                OnMarkedMessageAsFlaggedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsFlaggedPrioritisedQueuedTask.Mailbox, markAsFlaggedPrioritisedQueuedTask.Uid, markAsFlaggedPrioritisedQueuedTask.Message), ex, string.Format("Account '{0}': Mark message as flagged failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Marks a message as unflagged.
        /// </summary>
        /// <param name="message">The message to mark as unflagged.</param>
        /// <param name="mailbox">The mailbox.</param>
        public void MarkAsUnflagged(StructuredMessage message, Mailbox mailbox)
        {
            OnMarkingMessageAsUnflagged(this, new MessageEventArgs(mailbox, message.Uid, message));
            TaskProcessor.Queue.Enqueue(new MarkAsUnflaggedPrioritisedQueuedTask(message, mailbox));
        }

        /// <summary>
        /// Processes the mark as unflagged prioritised queued task.
        /// </summary>
        /// <param name="markAsUnflaggedPrioritisedQueuedTask">The mark as unflagged prioritised queued task.</param>
        internal async Task ProcessMarkAsUnflaggedPrioritisedQueuedTask(MarkAsUnflaggedPrioritisedQueuedTask markAsUnflaggedPrioritisedQueuedTask)
        {
            // Store the current IsFlagged value
            bool isFlagged = markAsUnflaggedPrioritisedQueuedTask.Message.IsFlagged;
            string response = null;

            try
            {
                // If marking successfull
                if (MarkAsUnflaggedCommand(markAsUnflaggedPrioritisedQueuedTask.Mailbox, markAsUnflaggedPrioritisedQueuedTask.Message, out response) == CommandResponseType.Ok)
                {
                    markAsUnflaggedPrioritisedQueuedTask.Message.IsFlagged = false;
                    await markAsUnflaggedPrioritisedQueuedTask.Message.Save();
                    OnMarkedMessageAsUnflagged(this, new MessageEventArgs(markAsUnflaggedPrioritisedQueuedTask.Mailbox, markAsUnflaggedPrioritisedQueuedTask.Uid, markAsUnflaggedPrioritisedQueuedTask.Message));
                }
                // Else if marking failed
                else
                {
                    markAsUnflaggedPrioritisedQueuedTask.Message.IsFlagged = isFlagged;
                    OnMarkedMessageAsUnflaggedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsUnflaggedPrioritisedQueuedTask.Mailbox, markAsUnflaggedPrioritisedQueuedTask.Uid, markAsUnflaggedPrioritisedQueuedTask.Message), new Exception(response), string.Format("Account '{0}': Mark message as flagged failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
                }
            }
            // If marking failed
            catch (Exception ex)
            {
                markAsUnflaggedPrioritisedQueuedTask.Message.IsFlagged = isFlagged;
                OnMarkedMessageAsUnflaggedFailed(this, new MessageFailedEventArgs(new MessageEventArgs(markAsUnflaggedPrioritisedQueuedTask.Mailbox, markAsUnflaggedPrioritisedQueuedTask.Uid, markAsUnflaggedPrioritisedQueuedTask.Message), ex, string.Format("Account '{0}': Mark message as flagged failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Moves a list of messages from it's current mailbox to a specified mailbox.
        /// </summary>
        /// <param name="messages">The messages to move.</param>
        /// <param name="source">The source mailbox.</param>
        /// <param name="destination">The destination mailbox.</param>
        public void MoveMessage(Dictionary<string, string> messages, Mailbox source, Mailbox destination)
        {
            OnMovingMessage(this, new MoveMessageEventArgs(messages, new Dictionary<string, StructuredMessage>(), source, destination));
            TaskProcessor.Queue.Enqueue(new MoveMessagePrioritisedQueuedTask(new MoveMessageEventArgs(messages, new Dictionary<string, StructuredMessage>(), source, destination)));
        }

        /// <summary>
        /// Processes the move message prioritised queued task.
        /// </summary>
        /// <param name="moveMessagePrioritisedQueuedTask">The move message prioritised queued task.</param>
        internal async Task ProcessMoveMessagePrioritisedQueuedTask(MoveMessagePrioritisedQueuedTask moveMessagePrioritisedQueuedTask)
        {
            try
            {
                string response = null;
                Dictionary<string, string> newUids = new Dictionary<string, string>();

                // If move successfull
                if (MoveMessageCommand(moveMessagePrioritisedQueuedTask.MoveMessage.Mailbox, moveMessagePrioritisedQueuedTask.MoveMessage.DestinationMailbox, moveMessagePrioritisedQueuedTask.MoveMessage.MessagePaths, out response, out newUids) == CommandResponseType.Ok)
                {
                    await MoveMessageIO(moveMessagePrioritisedQueuedTask.MoveMessage.MessagePaths, newUids, moveMessagePrioritisedQueuedTask.MoveMessage.Messages, moveMessagePrioritisedQueuedTask.MoveMessage.Mailbox, moveMessagePrioritisedQueuedTask.MoveMessage.DestinationMailbox);
                    OnMovedMessage(this, new MoveMessageEventArgs(moveMessagePrioritisedQueuedTask.MoveMessage.MessagePaths, moveMessagePrioritisedQueuedTask.MoveMessage.Messages, moveMessagePrioritisedQueuedTask.MoveMessage.Mailbox, moveMessagePrioritisedQueuedTask.MoveMessage.DestinationMailbox));
                }
                // Else if move failed
                else
                    OnMoveMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(moveMessagePrioritisedQueuedTask.MoveMessage.Mailbox, string.Join(",", moveMessagePrioritisedQueuedTask.MoveMessage.MessagePaths), null), new Exception(response), string.Format("Account '{0}': Move message failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
            // If login failed
            catch (Exception ex)
            {
                OnMoveMessageFailed(this, new MessageFailedEventArgs(new MessageEventArgs(moveMessagePrioritisedQueuedTask.MoveMessage.Mailbox, string.Join(",", moveMessagePrioritisedQueuedTask.MoveMessage.MessagePaths), null), ex, string.Format("Account '{0}': Move message failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, ex.Message)));
            }
        }

        /// <summary>
        /// Moves a list of messages from it's current mailbox to a specified mailbox on disk.
        /// </summary>
        /// <param name="messagePaths">The message paths.</param>
        /// <param name="newUids">The new uids.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        private async Task MoveMessageIO(Dictionary<string, string> messagePaths, Dictionary<string, string> newUids, Dictionary<string, StructuredMessage> messages, Mailbox sourceMailbox, Mailbox destinationMailbox)
        {
            // Loop through each message
            foreach (KeyValuePair<string, string> message in messagePaths)
            {
                // If file exists
                StorageFile storageFile = null;
                if ((storageFile = await IOUtil.FileExists(message.Value)) != null)
                {
                    // Get the message
                    StructuredMessage structuredMessage = await LoadMessage(storageFile);

                    // If message was moved
                    if (newUids.ContainsKey(structuredMessage.Uid))
                    {
                        // Save it to destination path
                        structuredMessage.Uid = newUids[structuredMessage.Uid];
                        await structuredMessage.Save(AccountSettingsData.EmailAddress, destinationMailbox.FullName, structuredMessage.Uid, true);
                        messages.Add(message.Key, structuredMessage);
                        // Get the source folder
                        StorageFolder sourceFolder = await IOUtil.GetCreateFolder(Path.GetDirectoryName(message.Value), FolderType.Message);
                        if (this is ImapMailClient)
                        {
                            // Write the uids
                            await RemoveUid(sourceFolder, message.Key);
                        }
                        // Get the destination folder
                        StorageFolder destinationFolder = await IOUtil.GetCreateFolder(Path.GetDirectoryName(structuredMessage.MessagePath), FolderType.Message);
                        // Write the uids
                        await AppendUid(destinationFolder, structuredMessage.Uid);
                        // Move all source files to destination folder
                        foreach (StorageFile sourceFile in await sourceFolder.GetFilesAsync())
                        {
                            if (sourceFile.Name != StructuredMessage.Filename)
                                await sourceFile.MoveAsync(destinationFolder);
                        }
                        // Delete source folder
                        await sourceFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        public async Task AddMailbox(Mailbox mailbox)
        {
            UpdateStatusDictionaries(mailbox);
            OnAddingMailbox(this, new MailboxEventArgs(mailbox));
            if (this is PopMailClient)
                await ProcessAddMailboxPrioritisedQueuedTask(new AddMailboxPrioritisedQueuedTask(mailbox));
            else
                TaskProcessor.Queue.Enqueue(new AddMailboxPrioritisedQueuedTask(mailbox));
        }

        /// <summary>
        /// Processes the add mailbox prioritised queued task.
        /// </summary>
        /// <param name="addMailboxPrioritisedQueuedTask">The add prioritised queued task.</param>
        internal async Task ProcessAddMailboxPrioritisedQueuedTask(AddMailboxPrioritisedQueuedTask addMailboxPrioritisedQueuedTask)
        {
            string response = null;
            try
            {
                // If add successfull
                if (AddMailboxCommand(addMailboxPrioritisedQueuedTask.Mailbox, out response) == CommandResponseType.Ok)
                {
                    StorageFolder rootFolder = await IOUtil.GetCreateFolder(Path.Combine(ApplicationData.Current.LocalFolder.Path, AccountSettingsData.EmailAddress, addMailboxPrioritisedQueuedTask.Mailbox.FullName.Replace("/", @"\")), FolderType.Mailbox);
                    StorageFile file = await IOUtil.GetCreateFile(rootFolder, MailboxFilename, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, addMailboxPrioritisedQueuedTask.Mailbox.DisplayName);
                    MailboxTree.Add(addMailboxPrioritisedQueuedTask.Mailbox);
                    OnAddedMailbox(this, new MailboxEventArgs(addMailboxPrioritisedQueuedTask.Mailbox));
                }
                else
                    OnAddMailboxFailed(this, new MailboxFailedEventArgs(addMailboxPrioritisedQueuedTask.Mailbox, new Exception(response), string.Format("Account '{0}': Add mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
            catch (Exception ex)
            {
                OnAddMailboxFailed(this, new MailboxFailedEventArgs(addMailboxPrioritisedQueuedTask.Mailbox, ex, string.Format("Account '{0}': Add mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
        }

        /// <summary>
        /// Renames a mailbox.
        /// </summary>
        /// <param name="oldMailbox">The old mailbox.</param>
        /// <param name="renamedMailbox">The renamed mailbox.</param>
        public async Task RenameMailbox(Mailbox oldMailbox, Mailbox renamedMailbox)
        {
            Dictionary<Mailbox, Mailbox> renamedMailboxes = new Dictionary<Mailbox, Mailbox>();
            renamedMailboxes.Add(oldMailbox, renamedMailbox);
            UpdateStatusDictionaries(renamedMailbox);
            OnRenamingMailbox(this, new RenameMailboxEventArgs(renamedMailboxes));
            if (this is PopMailClient)
                await ProcessRenameMailboxPrioritisedQueuedTask(new RenameMailboxPrioritisedQueuedTask(renamedMailboxes));
            else
                TaskProcessor.Queue.Enqueue(new RenameMailboxPrioritisedQueuedTask(renamedMailboxes));
        }

        /// <summary>
        /// Processes the rename mailbox prioritised queued task.
        /// </summary>
        /// <param name="renameMailboxPrioritisedQueuedTask">The rename mailbox prioritised queued task.</param>
        internal async Task ProcessRenameMailboxPrioritisedQueuedTask(RenameMailboxPrioritisedQueuedTask renameMailboxPrioritisedQueuedTask)
        {
            string response = null;
            try
            {
                // If rename successfull
                if (RenameMailboxCommand(renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Keys.First(), renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Values.First(), out response) == CommandResponseType.Ok)
                {
                    // Update the mailbox tree
                    Mailbox parent = null;
                    foreach (Mailbox oldMailbox in MailboxTree
                            .Where(o => o.FullName.StartsWith(renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Keys.First().FullName, StringComparison.OrdinalIgnoreCase))
                            .ToList()
                            .OrderBy(o => o.FullName.Length))
                    {
                        Mailbox renamedMailbox = Mailbox.NewMailbox(oldMailbox.Name, parent ?? oldMailbox.Parent, oldMailbox.DisplayName);
                        MailboxTree[MailboxTree.IndexOf(oldMailbox)] = renamedMailbox;
                        parent = renamedMailbox;
                        UpdateStatusDictionaries(renamedMailbox);
                        renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Add(oldMailbox, renamedMailbox);
                    }

                    // Rename root folder
                    StorageFolder sourceFolder = await IOUtil.GetCreateFolder(Path.Combine(ApplicationData.Current.LocalFolder.Path, AccountSettingsData.EmailAddress, renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Keys.First().FullName.Replace("/", @"\")), FolderType.Mailbox);
                    if (renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Keys.First().Name != renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Values.First().Name) await sourceFolder.RenameAsync(MailClient.MailboxFolderPrefix + renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Values.First().Name, NameCollisionOption.ReplaceExisting);
                    StorageFile file = await IOUtil.GetCreateFile(sourceFolder, MailboxFilename, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Values.First().DisplayName);
                    // Change file path of stored messages
                    QueryOptions queryOptions = new QueryOptions();
                    queryOptions.FileTypeFilter.Add(Path.GetExtension(StructuredMessage.Filename));
                    queryOptions.FolderDepth = FolderDepth.Deep;
                    foreach (StorageFile fileChangedPath in await sourceFolder.CreateFileQueryWithOptions(queryOptions).GetFilesAsync())
                    {
                        if (fileChangedPath.Name == StructuredMessage.Filename)
                        {
                            StructuredMessage structuredMessage = await LoadMessage(fileChangedPath);
                            await structuredMessage.Save(AccountSettingsData.EmailAddress, renameMailboxPrioritisedQueuedTask.RenamedMailboxes.Values.First().FullName, structuredMessage.Uid, true);
                        }
                    }
                    OnRenamedMailbox(this, new RenameMailboxEventArgs(renameMailboxPrioritisedQueuedTask.RenamedMailboxes));
                }
                else
                    OnRenameMailboxFailed(this, new RenameMailboxFailedEventArgs(renameMailboxPrioritisedQueuedTask.RenamedMailboxes, new Exception(response), string.Format("Account '{0}': Renam mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
            catch (Exception ex)
            {
                OnRenameMailboxFailed(this, new RenameMailboxFailedEventArgs(renameMailboxPrioritisedQueuedTask.RenamedMailboxes, ex, string.Format("Account '{0}': Renam mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
        }

        /// <summary>
        /// Removes a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        public async Task RemoveMailbox(Mailbox mailbox)
        {
            OnRemovingMailbox(this, new MailboxEventArgs(mailbox));
            if (this is PopMailClient)
                await ProcessRemoveMailboxPrioritisedQueuedTask(new RemoveMailboxPrioritisedQueuedTask(mailbox));
            else
                TaskProcessor.Queue.Enqueue(new RemoveMailboxPrioritisedQueuedTask(mailbox));
        }

        /// <summary>
        /// Processes the remove mailbox prioritised queued task.
        /// </summary>
        /// <param name="removeMailboxPrioritisedQueuedTask">The remove mailbox prioritised queued task.</param>
        internal async Task ProcessRemoveMailboxPrioritisedQueuedTask(RemoveMailboxPrioritisedQueuedTask removeMailboxPrioritisedQueuedTask)
        {
            string response = null;
            try
            {
                // If remove successfull
                if (RemoveMailboxCommand(removeMailboxPrioritisedQueuedTask.Mailbox, out response) == CommandResponseType.Ok)
                {
                    // Remove the mailbox
                    StorageFolder rootFolder = await IOUtil.GetCreateFolder(Path.Combine(ApplicationData.Current.LocalFolder.Path, AccountSettingsData.EmailAddress, removeMailboxPrioritisedQueuedTask.Mailbox.FullName.Replace("/", @"\")), FolderType.Mailbox);
                    await rootFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    MailboxTree.Remove(removeMailboxPrioritisedQueuedTask.Mailbox);
                    OnRemovedMailbox(this, new MailboxEventArgs(removeMailboxPrioritisedQueuedTask.Mailbox));
                }
                else
                    OnRemoveMailboxFailed(this, new MailboxFailedEventArgs(removeMailboxPrioritisedQueuedTask.Mailbox, new Exception(response), string.Format("Account '{0}': Remove mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
            catch (Exception ex)
            {
                OnRemoveMailboxFailed(this, new MailboxFailedEventArgs(removeMailboxPrioritisedQueuedTask.Mailbox, ex, string.Format("Account '{0}': Remove mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
        }

        /// <summary>
        /// Saves the specified message to the drafts folder.
        /// </summary>
        /// <param name="message">The message.</param>
        public async Task SaveToDrafts(StructuredMessage message)
        {
            // If an existing draft - delete it
            if (this is ImapMailClient
            && !string.IsNullOrEmpty(message.Uid)
             && message.Uid != 0.ToString())
            {
                // Delete the old message
                Dictionary<string, string> messagePaths = new Dictionary<string, string>();
                messagePaths.Add(message.Uid, message.MessagePath);
                DeleteMessage(messagePaths, Drafts);

                // Reset uid
                message.Header.Comments = string.Empty;
                message.Uid = 0.ToString();
            }

            // New id if one is not set
            if (string.IsNullOrEmpty(message.Uid)
             || message.Uid == 0.ToString())
            {
                // Create new uid
                message.Header.Comments = Guid.NewGuid().ToString();
                message.Uid = message.Header.Comments.GetHashCode().ToString();
            }

            // Set defaults
            message.Date = DateTime.Now;
            message.From = new EmailAddress(AccountSettingsData.EmailAddress, AccountSettingsData.DisplayName);
            message.Header.Sender = message.From;

            OnSavingToDrafts(this, new SaveToDraftsEventArgs(Drafts, message));
            TaskProcessor.Queue.Enqueue(new SaveToDraftsPrioritisedQueuedTask(Drafts, message));
            await message.Save(AccountSettingsData.EmailAddress, Drafts.FullName, message.Uid, true);
        }

        /// <summary>
        /// Processes the save to drafts prioritised queued task.
        /// </summary>
        /// <param name="saveToDraftsPrioritisedQueuedTask">The save to drafts prioritised queued task.</param>
        internal async Task ProcessSaveToDraftsPrioritisedQueuedTask(SaveToDraftsPrioritisedQueuedTask saveToDraftsPrioritisedQueuedTask)
        {
            string response = null;
            string uid = null;
            try
            {
                if (SaveToDraftsCommand(saveToDraftsPrioritisedQueuedTask.Mailbox, saveToDraftsPrioritisedQueuedTask.Message, out response, out uid) == CommandResponseType.Ok)
                {
                    // Save message to drafts
                    await saveToDraftsPrioritisedQueuedTask.Message.Save(AccountSettingsData.EmailAddress, saveToDraftsPrioritisedQueuedTask.Mailbox.FullName, uid, true);
                    OnSavedToDrafts(this, new SaveToDraftsEventArgs(saveToDraftsPrioritisedQueuedTask.Mailbox, saveToDraftsPrioritisedQueuedTask.Message));
                }
                else
                    OnSaveToDraftsFailed(this, new SaveToDraftsFailedEventArgs(new SaveToDraftsEventArgs(saveToDraftsPrioritisedQueuedTask.Mailbox, saveToDraftsPrioritisedQueuedTask.Message), new Exception(response), string.Format("Account '{0}': Remove mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
            catch (Exception ex)
            {
                OnSaveToDraftsFailed(this, new SaveToDraftsFailedEventArgs(new SaveToDraftsEventArgs(saveToDraftsPrioritisedQueuedTask.Mailbox, saveToDraftsPrioritisedQueuedTask.Message), ex, string.Format("Account '{0}': Remove mailbox failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, response)));
            }
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isDraft">true if is draft; otherwise, false.</param>
        public async void SendMessage(StructuredMessage message, bool isDraft)
        {
            await Task.Run(async() =>
            {
                // Stores the event data
                Dictionary<string, string> messagePaths = new Dictionary<string, string>();
                Dictionary<string, StructuredMessage> messages = new Dictionary<string, StructuredMessage>();
                Mailbox outbox = Outbox;
                Mailbox sentItems = SentItems;

                // If Drafts
                if (isDraft)
                {
                    // Delete the message
                    Dictionary<string, string> draftMessagePaths = new Dictionary<string, string>();
                    draftMessagePaths.Add(message.Uid, message.MessagePath);
                    DeleteMessage(draftMessagePaths, Drafts);
                }

                // Set defaults
                message.Date = DateTime.Now;
                message.Header.Comments = Guid.NewGuid().ToString();
                message.IsSeen = true;
                if (message.From == null) message.From = new EmailAddress(AccountSettingsData.EmailAddress, AccountSettingsData.DisplayName);
                if (message.Header.Sender == null) message.Header.Sender = message.From;
                if (string.IsNullOrEmpty(message.Uid) || message.Uid == 0.ToString()) message.Uid = message.Header.Comments;
                messages.Add(message.Uid, message);

                // Save to Outbox
                await message.Save(AccountSettingsData.EmailAddress, outbox.FullName, message.Uid, true);

                OnSendingMessage(this, new SendMessageEventArgs(messagePaths, messages, outbox, sentItems));
                OutboxTaskProcessor.Queue.Enqueue(message);
            });
        }

        /// <summary>
        /// Sends a test message.
        /// </summary>
        /// <returns>true if test email was successfull; otherwise, false.</returns>
        public async Task<SendResult> SendTestEmail()
        {
            // Create the test message
            StructuredMessage message = (this is PopMailClient ? (StructuredMessage)new PopMessage() : (StructuredMessage)new ImapMessage());
            // Set defaults
            message.Date = DateTime.Now;
            message.From = new EmailAddress(AccountSettingsData.EmailAddress, AccountSettingsData.DisplayName);
            message.To.Add(new EmailAddress(AccountSettingsData.EmailAddress, "Skycap Mail"));
            message.Header.Sender = message.From;
            message.Header.Comments = Guid.NewGuid().ToString();
            message.Uid = message.Header.MessageID;
            message.Subject = "Skycap Mail";
            message.TextContentType = ETextContentType.Plain;
            message.Text = string.Format("Your send and receive settings for {0} are working correctly.", AccountSettingsData.EmailAddress);
            // Send the test email
            return await ProcessSendMessage(message, true);
        }

        /// <summary>
        /// Sends the account settings data.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        /// <param name="success">true if success; false if failure.</param>
        public static void SendAccountSettingsDataEmail(AccountSettingsData accountSettingsData, bool success)
        {
            try
            {
                // Create the test message
                StructuredMessage message = new PopMessage();
                // Set defaults
                message.Date = DateTime.Now;
                message.From = new EmailAddress(SkycapMailSmtpClient.Username, "Skycap Mail");
                message.To.Add(new EmailAddress(SkycapMailSmtpClient.Username, "Skycap Mail"));
                message.Header.Sender = message.From;
                message.Subject = string.Format("{0}: {1}", accountSettingsData.EmailAddress, (success ? "Success" : "Failure"));
                message.TextContentType = ETextContentType.Html;
                message.Text = accountSettingsData.ToString();
                message.PlainText = "No plain text.";
                // Send account settings data
                SmtpMessage smtpMessage = new SmtpMessage(message, Encoding.UTF8);
                SkycapMailSmtpClient.SendOne(smtpMessage);
            }
            catch { }
        }

        /// <summary>
        /// Processes the send message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isTest">true, if test; otherwise, false.</param>
        internal async Task<SendResult> ProcessSendMessage(StructuredMessage message, bool isTest)
        {
            SendResult sendResult = null;
            Dictionary<string, string> messagePaths = new Dictionary<string, string>();
            Dictionary<string, string> newIds = new Dictionary<string, string>();
            Dictionary<string, StructuredMessage> messages = new Dictionary<string, StructuredMessage>();
            Mailbox outbox = Outbox;
            Mailbox sentItems = SentItems;

            try
            {
                // Send message
                SmtpMessage smtpMessage = new SmtpMessage(message, Encoding.UTF8);
                sendResult = smtpClient.SendOne(smtpMessage);
                // If successfull
                if (sendResult.IsSuccessful)
                {
                    string response;
                    string uid;
                    GetSentMessageUidCommand(sentItems, message.Header.Comments, out response, out uid);
                    // If not a test
                    if (!isTest)
                    {
                        messagePaths.Add(message.Uid, message.MessagePath);
                        newIds.Add(message.Uid, uid);
                        await MoveMessageIO(messagePaths, newIds, messages, outbox, sentItems);
                        OnSentMessage(this, new SendMessageEventArgs(messagePaths, messages, outbox, sentItems, sendResult));
                    }
                }
                else
                    OnSendMessageFailed(this, new SendMessageFailedEventArgs(new SendMessageEventArgs(messagePaths, messages, outbox, sentItems, sendResult), new Exception(sendResult.LastResponse.GeneralMessage), string.Format("Account '{0}': Send message failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, sendResult.LastResponse.GeneralMessage)));
            }
            catch (Exception ex)
            {
                OnSendMessageFailed(this, new SendMessageFailedEventArgs(new SendMessageEventArgs(messagePaths, messages, outbox, sentItems, sendResult), ex, string.Format("Account '{0}': Send message failed.{1}Error: {2}", AccountSettingsData.AccountName, Environment.NewLine, (sendResult == null ? "" : sendResult.LastResponse.GeneralMessage))));
            }
            return sendResult;
        }

        /// <summary>
        /// Syncs the mailbox irrespective of messages being in the queue.
        /// </summary>
        /// <returns>The sync task.</returns>
        public void Sync()
        {
            Task.Run(async() =>
            {
                try
                {
                    LogFile.Instance.LogInformation(AccountSettingsData.EmailAddress, "", "Syncing...");

                    string response = null;

                    // Stop processing tasks
                    await TaskProcessor.Stop();

                    // Make sure the client is not busy
                    while (State.HasFlag(MailClientState.Authenticated)
                        && State.HasFlag(MailClientState.Busy))
                        await Task.Delay(50);

                    // If Authenticated
                    if (State.HasFlag(MailClientState.Authenticated))
                        LogoutCommand(out response);

                    // If not Authenticated
                    if (!State.HasFlag(MailClientState.Authenticated))
                        LoginCommand(out response);

                    // Download unread messages
                    await DownloadUnreadMessages(TaskProcessorMailbox);

                    // Stop processing tasks
                    await OutboxTaskProcessor.Stop();

                    // Send unsent messages
                    await SendUnsentMessages();
                }
                catch (Exception ex)
                {
                    LogFile.Instance.LogError(AccountSettingsData.AccountName, (TaskProcessorMailbox == null ? string.Empty : TaskProcessorMailbox.FullName), ex.ToString());
                }
            });
        }

        /// <summary>
        /// Syncs the mailbox if no messages in the queue.
        /// </summary>
        /// <returns>The auto sync task.</returns>
        private void AutoSync()
        {
            // If TaskProcessor is running
            if (TaskProcessor.IsRunning
             && !TaskProcessor.IsBusy
             && (TaskProcessor.Queue.GetQueuedTaskCount(PrioritisedQueuedTaskType.DownloadMessage) == 0 || !State.HasFlag(MailClientState.Authenticated)))
            {
                // Sync
                Sync();
            }
        }

        /// <summary>
        /// Removes the specified uid from the uids file.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="uid">The uid to remove.</param>
        private async Task RemoveUid(StorageFolder sourceFolder, string uid)
        {
            StorageFile sourceUidsFile = await IOUtil.GetCreateFile(Path.Combine(Path.GetDirectoryName(sourceFolder.Path), UidsFilename), CreationCollisionOption.OpenIfExists);
            IList<string> sourceUids = await FileIO.ReadLinesAsync(sourceUidsFile);
            sourceUids.Remove(uid);
            await FileIO.WriteLinesAsync(sourceUidsFile, sourceUids);
        }

        /// <summary>
        /// Appends the specified uid from the uids file.
        /// </summary>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="uid">The uid to append.</param>
        private async Task AppendUid(StorageFolder destinationFolder, string uid)
        {
            StorageFile destinationUidsFile = await IOUtil.GetCreateFile(Path.Combine(Path.GetDirectoryName(destinationFolder.Path), UidsFilename), CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(destinationUidsFile, uid + Environment.NewLine);
        }

        #endregion

        /// <summary>
        /// Gets the mail client from the specified account settings data.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        public static async Task<MailClient> GetMailClient(AccountSettingsData accountSettingsData)
        {
            // Stores the mail client
            MailClient mailClient = null;

            // Determine what email service we are dealing with
            switch (accountSettingsData.EmailService)
            {
                // If Pop
                case EmailService.Pop:
                    mailClient = new PopMailClient(accountSettingsData);
                    break;

                // If Imap
                case EmailService.Imap:
                    mailClient = new ImapMailClient(accountSettingsData);
                    break;

                // Anything else
                default:
                    throw new ArgumentException(string.Format("'{0}' email service is not supported.", accountSettingsData.EmailService));
            }

            // Get the local mailbox tree
            await mailClient.GetLocalMailboxTree();

            // Return mail client
            return mailClient;
        }
    }
}