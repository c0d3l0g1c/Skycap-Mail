using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Skycap.Data;
using Skycap.IO;
using Skycap.Net.Common;
using Skycap.Net.Common.Configurations;
using Skycap.Net.Common.QueuedTasks;
using Skycap.Net.Imap.Collections;
using Skycap.Net.Pop3;
using Windows.Storage;

namespace Skycap.Net.Pop3
{
    /// <summary>
    /// Represents a pop mail client.
    /// </summary>
    public sealed class PopMailClient : MailClient
    {
        /// <summary>
        /// The pop3 mail client.
        /// </summary>
        private Pop3Client _client;
        /// <summary>
        /// The statistics.
        /// </summary>
        private IReadOnlyList<StatisticInfo> _statistics;

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Pop.PopMailClient class.
        /// </summary>
        /// <param name="accountSettingsData">The account settings data.</param>
        public PopMailClient(AccountSettingsData accountSettingsData)
            : base(accountSettingsData)
        {
            // Initialise local variables
            _client = new Pop3Client(accountSettingsData.AccountName, accountSettingsData.IncomingMailServer, accountSettingsData.IncomingMailServerPort, accountSettingsData.UserName, accountSettingsData.Password, (accountSettingsData.IsIncomingMailServerSsl ? EInteractionType.SSLPort : EInteractionType.Plain), EAuthenticationType.Auto);
            _client.Connected += client_Connected;
            _client.Disconnected += client_Disconnected;
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
                    switch (_client._connectionState)
                    {
                        // If Connected
                        case EPop3ConnectionState.Connected:
                            return MailClientState.Connected;

                        // If Loggined
                        case EPop3ConnectionState.Authenticated:
                            // Determine the client state
                            switch (_client._state)
                            {
                                // If Busy
                                case EPop3ClientState.Busy:
                                    return MailClientState.Authenticated | MailClientState.Busy;

                                // If Awaiting
                                default:
                                    return MailClientState.Authenticated | MailClientState.Awaiting;
                            }

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
                Pop3Response loginResponse = _client.Login();
                response = loginResponse.Message;
                return GetCommandResponseType(loginResponse.Type);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
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
                Pop3Response logoutResponse = _client.Logout();
                response = logoutResponse.Message;
                return GetCommandResponseType(logoutResponse.Type);
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
            response = string.Empty;
            return CommandResponseType.Ok;
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
                uint count = _client.GetStatistic().CountMessages;
                IOrderedEnumerable<Pop3MessageUIDInfo> serverUids = _client.GetAllUIDMessages().OrderByDescending(o => o.SerialNumber);
                Pop3MessageInfoCollection serverSizes = _client.GetMessagesInfo();
                response = string.Empty;
                statistics = serverUids.Join(serverSizes, u => u.SerialNumber, s => s.Number, (u, s) => new StatisticInfo(u.UniqueNumber, u.SerialNumber, s.Size, null)).OrderByDescending(o => o.SerialNumber).ToList();
                _statistics = statistics;
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
                message = _client.GetMessage(statisticInfo.SerialNumber, statisticInfo.UniqueNumber);
                if (!AccountSettingsData.KeepEmailCopiesOnServer)
                    _client.DeleteMessage(statisticInfo.SerialNumber);
                response = string.Empty;
                return CommandResponseType.Ok;
            }
            catch (Exception ex)
            {
                Login();
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
            response = string.Empty;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            newId = message.Uid;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            newUids = messagePaths.ToDictionary(o => o.Key, o => o.Key);
            return CommandResponseType.Ok;
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
            response = string.Empty;
            Pop3Response pop3Response = null;
            
            // Find the messages to delete
            foreach (StatisticInfo statistic in _statistics.Where(o => messagePaths.ContainsKey(o.UniqueNumber)))
            {
                // Delete the message
                pop3Response = _client.DeleteMessage(statistic.SerialNumber);
                response = pop3Response.Message;
            }

            return (pop3Response == null ? CommandResponseType.Ok : GetCommandResponseType(pop3Response.Type));
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
            response = string.Empty;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            return CommandResponseType.Ok;
        }

        /// <summary>
        /// Deletes a mailbox.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="response">The response.</param>
        /// <returns>The response type.</returns>
        protected override CommandResponseType RemoveMailboxCommand(Mailbox mailbox, out string response)
        {
            response = string.Empty;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            uid = message.Uid;
            return CommandResponseType.Ok;
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
            response = string.Empty;
            uid = messageId;
            return CommandResponseType.Bad;
        }

        /// <summary>
        /// Occurs when a mail client is connected to a mail server.
        /// </summary>
        /// <param name="sender">The object that raised the event (Pop3Client).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void client_Connected(Pop3Client sender)
        {
            OnConnected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when a mail client is disconnected from a mail server.
        /// </summary>
        /// <param name="sender">The object that raised the event (Pop3Client).</param>
        /// <param name="e">The event data (EventArgs).</param>
        private void client_Disconnected(Pop3Client sender)
        {
            OnDisconnected(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the command response type.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The command response type.</returns>
        private CommandResponseType GetCommandResponseType(EPop3ResponseType response)
        {
            // Determine response type
            switch (response)
            {
                // If OK
                case EPop3ResponseType.OK:
                    return CommandResponseType.Ok;

                // Else if error
                default:
                    return CommandResponseType.Bad;
            }
        }
    }
}
