using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Skycap.Data;
using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Net.Common.Configurations;
using Skycap.Net.Common.QueuedTasks;
using Skycap.Net.Imap;
using Skycap.Net.Imap.Collections;
using Skycap.Net.Imap.Responses;
using Skycap.Net.Imap.Sequences;
using Skycap.Net.Smtp;
using Windows.Storage;
using Windows.Storage.Search;

namespace Skycap.Net.Imap
{
    /// <summary>
    /// Represents an imap mail client.
    /// </summary>
    public sealed class ImapMailClient : MailClient
    {
        /// <summary>
        /// The imap mail client.
        /// </summary>
        private ImapClient _client;

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Pop.ImapMailClient class.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        public ImapMailClient(AccountSettingsData accountSettingsData)
            : base(accountSettingsData)
        {
            // Initialise local variables
            _client = new ImapClient();
            _client.Connected += client_Connected;
            _client.Disconnected += client_Disconnected;
            _client.AccountName = accountSettingsData.AccountName;
            _client.Host = accountSettingsData.IncomingMailServer;
            _client.Port = accountSettingsData.IncomingMailServerPort;
            _client.Username = accountSettingsData.UserName;
            _client.Password = accountSettingsData.Password;
            _client.SSLInteractionType = (accountSettingsData.IsIncomingMailServerSsl ? EInteractionType.SSLPort : EInteractionType.Plain);
            _client.AuthenticationType = EAuthenticationType.Auto;
        }

        /// <summary>
        /// Gets the mail client state.
        /// </summary>
        public override MailClientState State
        {
            get
            {
                lock (_client)
                {
                    // Determine the client state
                    switch (_client._state)
                    {
                        // If Connected
                        case EClientState.Connected:
                            return MailClientState.Connected;

                        // If Connected and Loggined
                        case EClientState.Connected | EClientState.Loggined:
                            return MailClientState.Authenticated | MailClientState.Awaiting;

                        // If Connected, Loggined and Selected
                        case EClientState.Connected | EClientState.Loggined | EClientState.Selected:
                            return MailClientState.Authenticated | MailClientState.Busy;

                        // If Disconnected
                        default:
                            return MailClientState.Disconnected;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the selected mailbox.
        /// </summary>
        public override Mailbox SelectedMailbox
        {
            get
            {
                return _client.Mailbox;
            }
        }

        /// <summary>
        /// Gets the attachment directory.
        /// </summary>
        public override string AttachmentDirectory
        {
            get
            {
                return _client.AttachmentDirectory;
            }
        }

        /// <summary>
        /// Logs in to the mail account.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType LoginCommand(out string response)
        {
            try
            {
                CompletionResponse loginResponse = _client.Login();
                response = loginResponse.Message;
                return GetCommandResponseType(loginResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.Message;
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Logs out of the mail account.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType LogoutCommand(out string response)
        {
            try
            {
                CompletionResponse logoutResponse = _client.Logout();
                response = logoutResponse.Message;
                return GetCommandResponseType(logoutResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Gets the remote mailbox tree hierachy.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType GetMailboxTreeCommand(out string response)
        {
            try
            {
                Mailbox mailboxTree = _client.GetMailboxTree();
                MailboxTree = new ObservableCollection<Mailbox>(GetFlatMailboxes(mailboxTree).OrderBy(o => (int)o.Folder).ThenBy(o => o.FullName));
                response = string.Empty;
                return CommandResponseType.Ok;
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Gets the mailbox statistics.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="response">The response.</param>
        /// <param name="statistics">The statistics.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType StatisticsCommand(Mailbox mailbox, out string response, out IReadOnlyList<StatisticInfo> statistics)
        {
            try
            {
                statistics = new List<StatisticInfo>(_client.GetStatistics(mailbox).OrderByDescending(o => o.SerialNumber));
                response = string.Empty;
                return CommandResponseType.Ok;
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                statistics = new List<StatisticInfo>();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Downloads a message.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="statisticInfo">The statistic info.</param>
        /// <param name="response">The response.</param>
        /// <param name="message">The message.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType DownloadMessageCommand(Mailbox mailbox, StatisticInfo statisticInfo, out string response, out StructuredMessage message)
        {
            try
            {
                message = _client.GetFullMessage(uint.Parse(statisticInfo.UniqueNumber), mailbox);
                response = string.Empty;
                return CommandResponseType.Ok;
            }
            catch (Exception ex)
            {
                LoginCommand(out response);
                message = null;
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Marks a message as read.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as read.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType MarkAsReadCommand(Mailbox mailbox, StructuredMessage message, out string response)
        {
            try
            {
                CompletionResponse markAsReadResponse = _client.MarkAsSeen(new SequenceNumber(uint.Parse(message.Uid)), mailbox);
                response = markAsReadResponse.Message;
                return GetCommandResponseType(markAsReadResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Marks a message as unread.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as unread.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType MarkAsUnreadCommand(Mailbox mailbox, StructuredMessage message, out string response)
        {
            try
            {
                CompletionResponse markAsUnreadResponse = _client.MarkAsUnseen(new SequenceNumber(uint.Parse(message.Uid)), mailbox);
                response = markAsUnreadResponse.Message;
                return GetCommandResponseType(markAsUnreadResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Marks a message as deleted.
        /// </summary>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        /// <param name="message">The message to mark as undelete.</param>
        /// <param name="response">The response.</param>
        /// <param name="newId">The new id.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType MarkAsDeletedCommand(Mailbox sourceMailbox, Mailbox destinationMailbox, StructuredMessage message, out string response, out string newId)
        {
            newId = null;
            try
            {
                Dictionary<string, string> newIds;
                CompletionResponse markAsDeleteResponse = _client.MarkAsDeleted(new SequenceNumber(uint.Parse(message.Uid)), sourceMailbox, destinationMailbox, out newIds);
                response = markAsDeleteResponse.Message;
                newId = newIds.Values.DefaultIfEmpty().First();
                return GetCommandResponseType(markAsDeleteResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                newId = string.Empty;
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Marks a message as undelete.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as undelete.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType MarkAsUndeletedCommand(Mailbox mailbox, StructuredMessage message, out string response)
        {
            try
            {
                CompletionResponse markAsUndeleteResponse = _client.MarkAsDeleted(new SequenceNumber(uint.Parse(message.Uid)), mailbox);
                response = markAsUndeleteResponse.Message;
                return GetCommandResponseType(markAsUndeleteResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Marks a message as flagged.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as flagged.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType MarkAsFlaggedCommand(Mailbox mailbox, StructuredMessage message, out string response)
        {
            try
            {
                CompletionResponse markAsFlaggedResponse = _client.MarkAsFlagged(new SequenceNumber(uint.Parse(message.Uid)), mailbox);
                response = markAsFlaggedResponse.Message;
                return GetCommandResponseType(markAsFlaggedResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Marks a message as unflagged.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message to mark as unflagged.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType MarkAsUnflaggedCommand(Mailbox mailbox, StructuredMessage message, out string response)
        {
            try
            {
                CompletionResponse markAsUnflaggedResponse = _client.MarkAsUnflagged(new SequenceNumber(uint.Parse(message.Uid)), mailbox);
                response = markAsUnflaggedResponse.Message;
                return GetCommandResponseType(markAsUnflaggedResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Moves a list of messages from it's current mailbox to a specified mailbox.
        /// </summary>
        /// <param name="sourceMailbox">The source mailbox.</param>
        /// <param name="destinationMailbox">The destination mailbox.</param>
        /// <param name="messagePaths">The message paths to move.</param>
        /// <param name="response">The response.</param>
        /// <param name="newUids">The new uids.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType MoveMessageCommand(Mailbox sourceMailbox, Mailbox destinationMailbox, Dictionary<string, string> messagePaths, out string response, out Dictionary<string, string> newUids)
        {
            newUids = new Dictionary<string, string>();

            try
            {
                SequenceSet sequenceSet = new SequenceSet(messagePaths.Keys.Select(o => new SequenceNumber(uint.Parse(o))));
                CompletionResponse moveMessageResponse = _client.Move(sequenceSet, sourceMailbox, destinationMailbox, out newUids);
                response = moveMessageResponse.Message;
                return GetCommandResponseType(moveMessageResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Deletes the specified message.
        /// </summary>
        /// <param name="mailbox">The mailbox to delete from.</param>
        /// <param name="messagePaths">The message paths to delete.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType DeleteMessageCommand(Mailbox mailbox, Dictionary<string, string> messagePaths, out string response)
        {
            try
            {
                // Email servers dont hav an outbox
                if (mailbox.Folder == MailboxFolders.Outbox)
                {
                    response = string.Empty;
                    return CommandResponseType.Ok;
                }
                // Any other mailbox
                else
                {
                    SequenceSet sequenceSet = new SequenceSet(messagePaths.Keys.Select(o => new SequenceNumber(uint.Parse(o))));
                    _client.MarkAsDeleted(sequenceSet, mailbox);
                    CompletionResponse deleteResponse = _client.Delete(sequenceSet, mailbox);
                    response = deleteResponse.Message;
                    return GetCommandResponseType(deleteResponse.CompletionResult);
                }
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Selects a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        protected override void SelectMailbox(Mailbox mailbox)
        {
            // Set folder
            _client.Mailbox = mailbox;
            // Set download directory
            _client.AttachmentDirectory = IOUtil.GetMailboxPath(AccountSettingsData.EmailAddress, mailbox.FullName);
        }

        /// <summary>
        /// Creates a new mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType AddMailboxCommand(Mailbox mailbox, out string response)
        {
            try
            {
                CompletionResponse addMailboxResponse = null;
                if (mailbox.Parent == null)
                    addMailboxResponse = _client.AddMailbox(mailbox.Name);
                else
                    addMailboxResponse = _client.AddMailbox(mailbox.Name, mailbox.Parent);
                response = addMailboxResponse.Message;
                return GetCommandResponseType(addMailboxResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Renames a mailbox.
        /// </summary>
        /// <param name="oldMailbox">The old mailbox.</param>
        /// <param name="renamedMailbox">The renamed mailbox.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType RenameMailboxCommand(Mailbox oldMailbox, Mailbox renamedMailbox, out string response)
        {
            try
            {
                CompletionResponse renameMailboxResponse = (renamedMailbox.IsSystem || renamedMailbox.IsReserved ? new CompletionResponse("0 OK SUCCESS") : _client.RenameMailbox(oldMailbox, renamedMailbox.DisplayName));
                response = renameMailboxResponse.Message;
                return GetCommandResponseType(renameMailboxResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Deletes a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType RemoveMailboxCommand(Mailbox mailbox, out string response)
        {
            try
            {
                CompletionResponse removeMailboxResponse = _client.DeleteMailbox(mailbox);
                response = removeMailboxResponse.Message;
                return GetCommandResponseType(removeMailboxResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Saves a message to drafts folder.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="message">The message.</param>
        /// <param name="response">The response.</param>
        /// <param name="uid">The uid.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType SaveToDraftsCommand(Mailbox mailbox, StructuredMessage message, out string response, out string uid)
        {
            try
            {
                CompletionResponse removeMailboxResponse = _client.Append(mailbox, message, out uid);
                response = removeMailboxResponse.Message;
                return GetCommandResponseType(removeMailboxResponse.CompletionResult);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                uid = string.Empty;
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Gets the sent message uid.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="response">The response.</param>
        /// <param name="uid">The uid.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType GetSentMessageUidCommand(Mailbox mailbox, string messageId, out string response, out string uid)
        {
            try
            {
                IEnumerable<uint> uids = _client.Search(Query.Header("Comments", messageId), mailbox);
                uid = uids.First().ToString();
                response = string.Empty;
                return CommandResponseType.Ok;
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                uid = string.Empty;
                return CommandResponseType.Bad;
            }
        }

        /// <summary>
        /// Gets a flat list of mailboxes from the mailbox tree.
        /// </summary>
        /// <param name="mailboxTree">The mail box tree.</param>
        /// <returns>A flat list of mailboxes</returns>
        private List<Mailbox> GetFlatMailboxes(Mailbox mailboxTree)
        {
            List<Mailbox> folders = new List<Mailbox>();
            Mailbox parentMailbox = null;
            folders = GetFlatMailboxes(folders, mailboxTree.Children);
            // Add the standard mailboxes
            AddStandardMailbox(folders, ref parentMailbox, MailboxFolders.Inbox);
            AddStandardMailbox(folders, ref parentMailbox, MailboxFolders.Drafts);
            AddStandardMailbox(folders, ref parentMailbox, MailboxFolders.Outbox);
            AddStandardMailbox(folders, ref parentMailbox, MailboxFolders.SentItems);
            AddStandardMailbox(folders, ref parentMailbox, MailboxFolders.JunkMail);
            AddStandardMailbox(folders, ref parentMailbox, MailboxFolders.DeletedItems);
            return folders;
        }

        /// <summary>
        /// Gets the list of mail folders.
        /// </summary>
        /// <param name="folders">The list of mail folders.</param>
        /// <param name="mailboxCollection">The mailbox children.</param>
        /// <returns>The list of mail folders.</returns>
        private List<Mailbox> GetFlatMailboxes(List<Mailbox> folders, MailboxCollection mailboxCollection)
        {
            // Loop through each mailbox
            foreach (Mailbox mailbox in mailboxCollection)
            {
                // Build the tree
                Task.Run(async () =>
                {
                    UpdateStatusDictionaries(mailbox);
                    folders.Add(mailbox);
                    SelectMailbox(mailbox);
                    StorageFolder folder = await IOUtil.GetCreateFolder(_client.AttachmentDirectory, FolderType.Mailbox);
                    StorageFile mailboxFile = await IOUtil.GetCreateFile(folder, MailboxFilename, CreationCollisionOption.OpenIfExists);
                    string displayName = await FileIO.ReadTextAsync(mailboxFile);
                    if (string.IsNullOrEmpty(displayName))
                        await FileIO.WriteTextAsync(mailboxFile, mailbox.DisplayName);
                    else if (mailbox.DisplayName != displayName)
                        mailbox.DisplayName = displayName;
                    GetFlatMailboxes(folders, mailbox.Children);
                }).Wait();
            }
            // Return list
            return folders;
        }

        /// <summary>
        /// Occurs when a mail client is connected to a mail server.
        /// </summary>
        /// <param name="sender">The object that raised the event (ImapClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void client_Connected(ImapClient sender)
        {
            OnConnected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when a mail client is disconnected from a mail server.
        /// </summary>
        /// <param name="sender">The object that raised the event (ImapClient).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void client_Disconnected(ImapClient sender)
        {
            OnDisconnected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the command response type.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The command response type.</returns>
        private CommandResponseType GetCommandResponseType(ECompletionResponseType response)
        {
            // Determine response type
            switch (response)
            {
                // If OK
                case ECompletionResponseType.OK:
                    return CommandResponseType.Ok;

                // If NO
                case ECompletionResponseType.NO:
                    return CommandResponseType.No;

                // Else if error
                default:
                    return CommandResponseType.Bad;
            }
        }
    }
}
