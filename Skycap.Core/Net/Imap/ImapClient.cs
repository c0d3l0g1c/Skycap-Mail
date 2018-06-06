namespace Skycap.Net.Imap
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;

    using Skycap.Net.Common;
    using Skycap.Net.Smtp;
    using Skycap.Net.Common.Configurations;
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Common.Exceptions;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Imap.Collections;
    using Skycap.Net.Imap.Commands;
    using Skycap.Net.Imap.Events;
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Parsers;
    using Skycap.Net.Imap.Responses;
    using Skycap.Net.Imap.Scripts;
    using Skycap.Net.Imap.Sequences;
    using Windows.Storage;

    public class ImapClient
    {
        private Mailbox _activeMailbox;
        protected CodeConfigurationProvider _configurationProvider;
        protected IConnection _connection;
        protected IConnectionFactory _connectionFactory;
        protected const string _defaultHierarchyDelimiter = "/";
        protected IInteractDispatcher _dispatcher;
        protected const string _exBadAsyncResult = "Bad AsyncResult object";
        protected const string _exCapabilityError = "Cannot obtain capability list";
        protected const string _exCollectionOfUidCannotBeNull = "Collection of uid cannot be null";
        protected const string _exDestinationMailboxCannotBeNull = "Destination mailbox cannot be null";
        protected const string _exEndUIDLessStartUID = "UID of end message cannot be less then UID of start message";
        protected const string _exFailToSelect = "Mailbox doesn't exist or you can't access it";
        protected const string _exHierarchyDelimiterInMailbox = "Mailbox name can't contains hierarchy delimiter symbol";
        protected const string _exInvalidMailboxName = "Mailbox name can't contains hierarchy delimiter symbol";
        protected const string _exMailboxCannotBeNull = "Mailbox cannot be null";
        protected const string _exMailboxNameRequired = "Mailbox name can't be null or empty";
        protected const string _exMessageSequenceCannotBeNull = "Message sequence cannot be null";
        protected const string _exNullFlag = "Flags can't be null";
        protected const string _exNullMailbox = "Mailbox can't be null";
        protected const string _exNullQuery = "Query can't be null";
        protected const string _exNullRootMailbox = "Root mailbox can't be null";
        protected const string _exSourceMailboxCannotBeNull = "Source mailbox cannot be null";
        protected const string _InboxMailbox = "INBOX";
        protected string _login;
        protected string _password;
        internal EClientState _state;
        protected const uint _taskSize = 0x29a;
        protected const string exIncorrectLogin = "Login can't be null or empty string";
        protected const string exIncorrectPassword = "Password can't be null or empty string";
        protected const string exLoginRequired = "This action not allowed in current state";
        protected const string exUnknownProxyType = "Current ProxyType not supported";
        private Regex AppendUidRegex = new Regex(@"APPENDUID (\d+) (\d+)", RegexOptions.IgnoreCase);

        public event ClientEventHandler AllMessagesReceived;

        public event AttachReceivedEventHandler AttachReceived;

        public event ClientEventHandler Authentificated;

        public event BrokenMessageEventHandler BrokenMessage;

        public event ClientEventHandler Completed;

        public event StateChangedEventHandler ExistsChanged;

        public event StateChangedEventHandler MailboxStatusChanged;

        public event MessageReceivedEventHandler MessageHeaderReceived;

        public event MessageReceivedEventHandler MessageReceived;

        public event ClientEventHandler Quit;

        public event ClientEventHandler Connected;

        public event ClientEventHandler Disconnected;

        public event StateChangedEventHandler RecentChanged;

        public event Skycap.Net.Imap.Events.ServerResponseReceived ServerResponseReceived;

        public ImapClient()
        {
            this._state = EClientState.Disconnected;
            this.InitializeInternalStructure();
        }

        private void _dispatcher_ServerResponseReceived(object sender, ServerResponseReceivedEventArgs e)
        {
            switch (this.GetSuddenResponseType(e.ReceivedResponse))
            {
                case ESuddenResponseType.Exists:
                    this.OnExistsChanged();
                    return;

                case ESuddenResponseType.Recent:
                    this.OnRecentChanged();
                    return;

                case ESuddenResponseType.Status:
                    this.OnMailboxStatusChanged();
                    return;
            }
            this.OnServerResponseReceived(e.ReceivedResponse);
        }

        protected virtual void CheckState(EClientState targetState)
        {
            if ((this._state & targetState) == 0)
            {
                throw new InvalidOperationException("This action not allowed in current state");
            }
        }

        public virtual CompletionResponse Copy(ISequence uidSequence, Mailbox source, Mailbox destination)
        {
            CompletionResponse response3;
            if (uidSequence == null)
            {
                throw new ArgumentNullException("uidSequence", "Message sequence cannot be null");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source mailbox cannot be null");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination", "Destination mailbox cannot be null");
            }
            try
            {
                if (this.Select(source).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                this.CheckState(EClientState.Selected | EClientState.Loggined);
                CompletionResponse response2 = new UIDCOPYCommand(uidSequence, destination.Name).Interact(this._dispatcher);
                this.OnCompleted();
                response3 = response2;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response3;
        }

        protected virtual void CreateDispatcher()
        {
            this._connection = this._connectionFactory.GetConnection(this._configurationProvider);
            if (this._dispatcher != null)
            {
                this._dispatcher.ServerResponseReceived -= new EventHandler<ServerResponseReceivedEventArgs>(this._dispatcher_ServerResponseReceived);
            }
            this._dispatcher = new InteractDispatcher(this._connection);
            this._dispatcher.ServerResponseReceived += new EventHandler<ServerResponseReceivedEventArgs>(this._dispatcher_ServerResponseReceived);
        }

        public virtual CompletionResponse AddMailbox(string folderName)
        {
            CompletionResponse response2;
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentException("Mailbox name can't be null or empty", "folderName");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                CompletionResponse response = new CREATECommand(StringEncoding.EncodeMailboxName(folderName)).Interact(this._dispatcher);
                this.OnCompleted();
                response2 = response;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response2;
        }

        public virtual CompletionResponse AddMailbox(string folderName, Mailbox rootMailbox)
        {
            if (rootMailbox == null)
            {
                throw new ArgumentNullException("rootMailbox", "Root mailbox can't be null");
            }
            if (folderName.Contains(rootMailbox.HierarchyDelimiter))
            {
                throw new FormatException("Mailbox name can't contains hierarchy delimiter symbol");
            }
            string str = string.Format("{0}{1}{2}", rootMailbox.FullName, rootMailbox.HierarchyDelimiter, folderName);
            return this.AddMailbox(str);
        }

        public virtual CompletionResponse DeleteMailbox(Mailbox mailbox)
        {
            CompletionResponse response2;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                this.SelectInbox();
                CompletionResponse response = new DELETECommand(mailbox.FullName).Interact(this._dispatcher);
                this.OnCompleted();
                response2 = response;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response2;
        }

        public virtual CompletionResponse DeleteMarkedMessages(Mailbox mailbox)
        {
            CompletionResponse response3;
            try
            {
                EXPUNGECommand command = new EXPUNGECommand();
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                CompletionResponse response2 = command.Interact(this._dispatcher);
                this.OnCompleted();
                response3 = response2;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response3;
        }

        protected void Dispose(bool disposing)
        {

        }

        private static TDelegate ExtractAsyncCaller<TDelegate>(IAsyncResult result) where TDelegate: class
        {
            if (result == null)
            {
                throw new ArgumentNullException("result", "Result can't be null");
            }
            TDelegate asyncDelegate = ((IAsyncResult) result).AsyncState as TDelegate;
            if (asyncDelegate == null)
            {
                throw new ArgumentException("Bad AsyncResult object", "result");
            }
            return asyncDelegate;
        }

        private void FetchCommand_BrokenMessage(object sender, BrokenMessageInfoArgs e)
        {
            this.OnBrokenMessage(e);
        }

        private void FetchCommand_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            this.OnMessageHeaderReceived(args.Message);
        }

        public virtual MessageCollection GetAllMessageHeaders(Mailbox mailbox)
        {
            MessageCollection messages2;
            this.CheckState(EClientState.Loggined);
            try
            {
                CompletionResponse response = this.Select(mailbox);
                MessageCollection messages = new MessageCollection();
                if (response.CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                uint messageCount = this.GetMessageCount(mailbox);
                for (uint i = 0; i < Math.Ceiling((double) (((double) messageCount) / 666.0)); i++)
                {
                    uint number = (i * 0x29a) + 1;
                    uint num4 = (i + 1) * 0x29a;
                    num4 = Math.Min(num4, messageCount);
                    ReceiveHeader header = new ReceiveHeader(new SequenceRange(new SequenceNumber(number), new SequenceNumber(num4)), false, this._configurationProvider.AttachmentDirectory);
                    header.MessageReceived += new EventHandler<MessageReceivedEventArgs>(this.FetchCommand_MessageReceived);
                    header.BrokenMessage += new EventHandler<BrokenMessageInfoArgs>(this.FetchCommand_BrokenMessage);
                    header.Interact(this._dispatcher);
                    header.BrokenMessage -= new EventHandler<BrokenMessageInfoArgs>(this.FetchCommand_BrokenMessage);
                    header.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(this.FetchCommand_MessageReceived);
                    this.Noop();
                    messages.AddRange(header.MessageCollection);
                }
                this.OnAllMessagesReceived();
                this.OnCompleted();
                messages2 = messages;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return messages2;
        }

        public virtual Attachment GetAttachment(Mailbox mailbox, ImapMessage message, Attachment attachment)
        {
            Attachment attachment2;
            this.CheckState(EClientState.Loggined);
            try
            {
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                this._configurationProvider.AttachmentDirectory = Path.Combine(this._configurationProvider.AttachmentDirectory, message.Uid);
                new ReceiveAttach(message, attachment, this._configurationProvider.AttachmentDirectory).Interact(this._dispatcher);
                this.OnAttachmentReceived(new AttachReceivedArgs(message, attachment));
                this.OnCompleted();
                attachment2 = attachment;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return attachment2;
        }

        public virtual ImapMessage GetFullMessage(uint uid, Mailbox mailbox)
        {
            ImapMessage message2;
            this.CheckState(EClientState.Loggined);
            try
            {
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                ReceiveFullMessage message = new ReceiveFullMessage(new SequenceNumber(uid), this._configurationProvider.AttachmentDirectory);
                message.Run(this._dispatcher);
                if (message.Message != null)
                {
                    this.OnMessageRecived(message.Message);
                }
                this.OnCompleted();
                message2 = message.Message;
                this._state = EClientState.Connected | EClientState.Loggined;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return message2;
        }

        public virtual MailboxCollection GetMailboxCollection()
        {
            return this.GetMailboxTree().Children;
        }

        public virtual Mailbox GetMailboxTree()
        {
            Mailbox mailbox2;
            this.CheckState(EClientState.Loggined);
            try
            {
                LISTCommand command = new LISTCommand("", "*");
                command.Interact(this._dispatcher);
                Mailbox mailbox = Mailbox.BuildTree(command.MatchedNames);
                this.OnCompleted();
                mailbox2 = mailbox;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return mailbox2;
        }

        public virtual uint GetMessageCount(Mailbox mailbox)
        {
            uint num;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                STATUSCommand command = new STATUSCommand(mailbox.FullName, new EIMAP4StatusRequest[] { EIMAP4StatusRequest.Messages });
                command.Interact(this._dispatcher);
                this.OnCompleted();
                num = Convert.ToUInt32(command.Messages);
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return num;
        }

        public virtual uint GetMailboxSize(Mailbox mailbox)
        {
            uint size;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                QUOTAROOTCommand command = new QUOTAROOTCommand(mailbox);
                command.Interact(this._dispatcher);
                this.OnCompleted();
                size = command.Size;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return size;
        }

        public virtual IEnumerable<StatisticInfo> GetStatistics(Mailbox mailbox)
        {
            List<StatisticInfo> statistics = new List<StatisticInfo>();
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                ReceiveStats command = new ReceiveStats(mailbox);
                command.Interact(this._dispatcher);
                statistics = new List<StatisticInfo>(command.Statistics);
                this.OnCompleted();
            }
            catch (Exception exception)
            {
                //this.HandleException(exception);
                throw;
            }
            return statistics.AsEnumerable();
        }

        public virtual MessageCollection GetMessagesHeader(Mailbox mailbox, IEnumerable<uint> uids)
        {
            MessageCollection messages2;
            if (uids == null)
            {
                throw new ArgumentNullException("uids", "Collection of uid cannot be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                CompletionResponse response = this.Select(mailbox);
                MessageCollection messages = new MessageCollection();
                if (response.CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                List<uint> list = new List<uint>(uids);
                int count = list.Count;
                for (int i = 0; i < Math.Ceiling((double) (((double) count) / 666.0)); i++)
                {
                    SequenceSet range = new SequenceSet();
                    int num3 = (int) (i * 0x29aL);
                    int num4 = (int) ((i + 1) * 0x29aL);
                    num4 = (num4 > list.Count) ? list.Count : num4;
                    for (int j = num3; j < num4; j++)
                    {
                        range.Add(new SequenceNumber(list[j]));
                    }
                    ReceiveHeader header = new ReceiveHeader(range, true, this._configurationProvider.AttachmentDirectory);
                    header.MessageReceived += new EventHandler<MessageReceivedEventArgs>(this.FetchCommand_MessageReceived);
                    header.BrokenMessage += new EventHandler<BrokenMessageInfoArgs>(this.FetchCommand_BrokenMessage);
                    header.Interact(this._dispatcher);
                    header.BrokenMessage -= new EventHandler<BrokenMessageInfoArgs>(this.FetchCommand_BrokenMessage);
                    header.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(this.FetchCommand_MessageReceived);
                    this.Noop();
                    messages.AddRange(header.MessageCollection);
                }
                this.OnAllMessagesReceived();
                this.OnCompleted();
                messages2 = messages;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return messages2;
        }

        public virtual MessageCollection GetMessagesHeader(Mailbox mailbox, uint start, uint end)
        {
            MessageCollection messages2;
            if (end > start)
            {
                throw new ArgumentException("UID of end message cannot be less then UID of start message", "end");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                CompletionResponse response = this.Select(mailbox);
                MessageCollection messages = new MessageCollection();
                if (response.CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                uint num = end - start;
                for (uint i = 0; i < Math.Ceiling((double) (((double) num) / 666.0)); i++)
                {
                    uint number = (i * 0x29a) + 1;
                    uint num4 = (i + 1) * 0x29a;
                    SequenceRange range = new SequenceRange(new SequenceNumber(number), new SequenceNumber(num4));
                    ReceiveHeader header = new ReceiveHeader(range, true, this._configurationProvider.AttachmentDirectory);
                    header.MessageReceived += new EventHandler<MessageReceivedEventArgs>(this.FetchCommand_MessageReceived);
                    header.BrokenMessage += new EventHandler<BrokenMessageInfoArgs>(this.FetchCommand_BrokenMessage);
                    header.Interact(this._dispatcher);
                    header.BrokenMessage -= new EventHandler<BrokenMessageInfoArgs>(this.FetchCommand_BrokenMessage);
                    header.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(this.FetchCommand_MessageReceived);
                    this.Noop();
                    messages.AddRange(header.MessageCollection);
                }
                this.OnAllMessagesReceived();
                this.OnCompleted();
                messages2 = messages;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return messages2;
        }

        public virtual ImapMessage GetMessageText(uint uid, Mailbox mailbox)
        {
            ImapMessage message;
            this.CheckState(EClientState.Loggined);
            try
            {
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                ReceiveMessageText text = new ReceiveMessageText(new SequenceNumber(uid), this._configurationProvider.AttachmentDirectory);
                text.Run(this._dispatcher);
                if (text.Message != null)
                {
                    this.OnMessageRecived(text.Message);
                }
                this.OnCompleted();
                message = text.Message;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return message;
        }

        public virtual MailboxCollection GetSubscribedMailboxCollection()
        {
            return this.GetSubscribedMailboxTree().Children;
        }

        public virtual Mailbox GetSubscribedMailboxTree()
        {
            Mailbox mailbox2;
            this.CheckState(EClientState.Loggined);
            try
            {
                LSUBCommand command = new LSUBCommand("", "*");
                command.Interact(this._dispatcher);
                Mailbox mailbox = Mailbox.BuildTree(command.MatchedNames);
                this.OnCompleted();
                mailbox2 = mailbox;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return mailbox2;
        }

        protected ESuddenResponseType GetSuddenResponseType(IMAP4Response response)
        {
            switch (response.Name.ToLower())
            {
                case "exists":
                    return ESuddenResponseType.Exists;

                case "recent":
                    return ESuddenResponseType.Recent;

                case "status":
                    return ESuddenResponseType.Status;
            }
            return ESuddenResponseType.Other;
        }

        protected virtual List<string> GetSupportedAuthenticationTypes()
        {
            List<string> list = new List<string>();
            CAPABIILITYCommand command = new CAPABIILITYCommand();
            CompletionResponse response = command.Interact(this._dispatcher);
            Regex regex = new Regex(@"AUTH=(?<method>[A-Za-z0-9\-]+)");
            if (response.CompletionResult != ECompletionResponseType.OK)
            {
                throw new ImapException("Cannot obtain capability list");
            }
            foreach (string str in command.Capabilities)
            {
                if (regex.IsMatch(str))
                {
                    list.Add(regex.Match(str).Groups["method"].Value);
                }
            }
            return list;
        }

        private void HandleException(Exception e)
        {
            if (((e is IOException) || (e is ConnectionException) || (e is Exception)))
            {
                this._connection.Close();
                this._state = EClientState.Disconnected;
                if (Disconnected != null) Disconnected(this);
            }
        }

        protected void InitializeInternalStructure()
        {
            this._configurationProvider = new CodeConfigurationProvider("localhost", 0x19);
            this._connectionFactory = new ConnectionFactory();
        }

        public virtual CompletionResponse Login()
        {
            CompletionResponse response;
            this.CheckState(EClientState.Disconnected);
            this.CreateDispatcher();
            try
            {
                BaseAUTHENTICATECommand command;
                this._dispatcher.Open();
                this._state = EClientState.Connected;
                if (this._configurationProvider.SSLInteractionType == EInteractionType.StartTLS)
                {
                    new STARTTLSCommand().Interact(this._dispatcher);
                    this._dispatcher.SwitchToSslChannel();
                }
                List<string> supportedAuthenticationTypes = this.GetSupportedAuthenticationTypes();
                if (this._configurationProvider.AuthenticationType == EAuthenticationType.Auto)
                {
                    command = AuthenticationMethodFactory.GetBestAuthenticateCommand(supportedAuthenticationTypes, this._login, this._password);
                }
                else
                {
                    if (!supportedAuthenticationTypes.Contains(AuthenticationMethodFactory.GetAuthenticationMethodName(this._configurationProvider.AuthenticationType)))
                    {
                        throw new AuthenticationMethodNotSupportedException(AuthenticationMethodFactory.GetAuthenticationMethodName(this._configurationProvider.AuthenticationType));
                    }
                    command = AuthenticationMethodFactory.CreateAuthenticateCommand(this._configurationProvider.AuthenticationType, this._login, this._password);
                }
                response = command.Interact(this._dispatcher);
                if (response.CompletionResult != ECompletionResponseType.OK)
                {
                    throw new ImapException(string.Format("Fail to login: {0}", response.Message));
                }
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            this._state |= EClientState.Loggined;
            this.OnCompleted();
            this.OnAuthentificated();
            return response;
        }

        public virtual CompletionResponse Logout()
        {
            CompletionResponse response2;
            this.CheckState(EClientState.Loggined);
            try
            {
                CompletionResponse response = new LOGOUTCommand().Interact(this._dispatcher);
                this._state = EClientState.Disconnected;
                this._dispatcher.Close();
                this.OnQuit();
                this.OnCompleted();
                response2 = response;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response2;
        }

        public virtual CompletionResponse MarkAsDeleted(ISequence uidSequence, Mailbox mailbox)
        {
            MessageFlagCollection flags = new MessageFlagCollection {
                new MessageFlag(EFlag.Seen),
                new MessageFlag(EFlag.Deleted)
            };
            return this.SetMessageFlags(uidSequence, mailbox, flags, EFlagMode.Add);
        }

        public virtual CompletionResponse MarkAsSeen(ISequence uidSequence, Mailbox mailbox)
        {
            MessageFlagCollection flags = new MessageFlagCollection {
                new MessageFlag(EFlag.Seen)
            };
            return this.SetMessageFlags(uidSequence, mailbox, flags, EFlagMode.Add);
        }

        public virtual CompletionResponse MarkAsUndeleted(ISequence uidSequence, Mailbox mailbox)
        {
            MessageFlagCollection flags = new MessageFlagCollection {
                new MessageFlag(EFlag.Answered)
            };
            return this.SetMessageFlags(uidSequence, mailbox, flags, EFlagMode.Remove);
        }

        public virtual CompletionResponse MarkAsUnseen(ISequence uidSequence, Mailbox mailbox)
        {
            MessageFlagCollection flags = new MessageFlagCollection {
                new MessageFlag(EFlag.Seen)
            };
            return this.SetMessageFlags(uidSequence, mailbox, flags, EFlagMode.Remove);
        }

        public virtual CompletionResponse MarkAsFlagged(ISequence uidSequence, Mailbox mailbox)
        {
            MessageFlagCollection flags = new MessageFlagCollection {
                new MessageFlag(EFlag.Flagged)
            };
            return this.SetMessageFlags(uidSequence, mailbox, flags, EFlagMode.Add);
        }

        public virtual CompletionResponse MarkAsUnflagged(ISequence uidSequence, Mailbox mailbox)
        {
            MessageFlagCollection flags = new MessageFlagCollection {
                new MessageFlag(EFlag.Flagged)
            };
            return this.SetMessageFlags(uidSequence, mailbox, flags, EFlagMode.Remove);
        }

        public virtual CompletionResponse MarkAsDeleted(ISequence uidSequence, Mailbox source, Mailbox destination, out Dictionary<string, string> newUids)
        {
            newUids = new Dictionary<string, string>();
            CompletionResponse response3;
            if (uidSequence == null)
            {
                throw new ArgumentNullException("uidSequence", "Message sequence cannot be null");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source mailbox cannot be null");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination", "Destination mailbox cannot be null");
            }
            try
            {
                if (this.Select(source).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                this.CheckState(EClientState.Selected | EClientState.Loggined);
                CompletionResponse response2 = new COPYCommand(uidSequence, destination.FullName).Interact(this._dispatcher);
                if (response2.CompletionResult == ECompletionResponseType.OK)
                {
                    newUids = GetCopyResponse(response2.Message);
                    uidSequence = new SequenceSet(newUids.Values.Select(o => new SequenceNumber(uint.Parse(o))));
                    response2 = MarkAsDeleted(uidSequence, destination);
                }
                this.OnCompleted();
                response3 = response2;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response3;
        }

        public virtual CompletionResponse Move(ISequence uidSequence, Mailbox source, Mailbox destination, out Dictionary<string, string> newUids)
        {
            newUids = new Dictionary<string, string>();
            CompletionResponse response3;
            if (uidSequence == null)
            {
                throw new ArgumentNullException("uidSequence", "Message sequence cannot be null");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source mailbox cannot be null");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination", "Destination mailbox cannot be null");
            }
            try
            {
                if (this.Select(source).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                this.CheckState(EClientState.Selected | EClientState.Loggined);
                CompletionResponse response2 = new COPYCommand(uidSequence, destination.FullName).Interact(this._dispatcher);
                if (response2.CompletionResult == ECompletionResponseType.OK)
                {
                    newUids = GetCopyResponse(response2.Message);
                    response2 = new STORECommand(uidSequence, new List<string>() { EFlag.Deleted.ToString() }.AsEnumerable(), EFlagMode.Add).Interact(this._dispatcher);
                    if (response2.CompletionResult == ECompletionResponseType.OK)
                        response2 = new UIDEXPUNGECommand(uidSequence).Interact(this._dispatcher);
                }
                this.OnCompleted();
                response3 = response2;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response3;
        }

        public virtual CompletionResponse Delete(ISequence uidSequence, Mailbox source)
        {
            CompletionResponse response3;
            if (uidSequence == null)
            {
                throw new ArgumentNullException("uidSequence", "Message sequence cannot be null");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source mailbox cannot be null");
            }
            try
            {
                if (this.Select(source).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                this.CheckState(EClientState.Selected | EClientState.Loggined);
                CompletionResponse response2 = new UIDEXPUNGECommand(uidSequence).Interact(this._dispatcher);
                this.OnCompleted();
                response3 = response2;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response3;        
        }

        public virtual CompletionResponse Expunge(ISequence uidSequence, Mailbox source)
        {
            CompletionResponse response2;
            if (uidSequence == null)
            {
                throw new ArgumentNullException("uidSequence", "Message sequence cannot be null");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source", "Source mailbox cannot be null");
            }
            try
            {
                if (this.Select(source).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                this.CheckState(EClientState.Selected | EClientState.Loggined);
                CompletionResponse response = new UIDEXPUNGECommand(uidSequence).Interact(this._dispatcher);
                this.OnCompleted();
                response2 = response;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response2;
        }

        public virtual CompletionResponse Noop()
        {
            CompletionResponse response;
            try
            {
                response = new NOOPCommand().Interact(this._dispatcher);
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response;
        }

        protected virtual void OnAllMessagesReceived()
        {
            ClientEventHandler allMessagesReceived = this.AllMessagesReceived;
            if (allMessagesReceived != null)
            {
                allMessagesReceived(this);
            }
        }

        protected void OnAttachmentReceived(AttachReceivedArgs args)
        {
            AttachReceivedEventHandler attachReceived = this.AttachReceived;
            if (attachReceived != null)
            {
                attachReceived(this, args);
            }
        }

        protected void OnAuthentificated()
        {
            ClientEventHandler authentificated = this.Authentificated;
            if (authentificated != null)
            {
                authentificated(this);
            }
        }

        protected void OnBrokenMessage(BrokenMessageInfoArgs args)
        {
            BrokenMessageEventHandler brokenMessage = this.BrokenMessage;
            if (brokenMessage != null)
            {
                brokenMessage(this, args);
            }
        }

        protected virtual void OnCompleted()
        {
            ClientEventHandler completed = this.Completed;
            if (completed != null)
            {
                completed(this);
            }
        }

        protected virtual void OnExistsChanged()
        {
            StateChangedEventHandler existsChanged = this.ExistsChanged;
            if (existsChanged != null)
            {
                existsChanged(this, this._activeMailbox);
            }
        }

        protected virtual void OnMailboxStatusChanged()
        {
            StateChangedEventHandler mailboxStatusChanged = this.MailboxStatusChanged;
            if (mailboxStatusChanged != null)
            {
                mailboxStatusChanged(this, this._activeMailbox);
            }
        }

        protected void OnMessageHeaderReceived(ImapMessage message)
        {
            MessageReceivedEventHandler messageHeaderReceived = this.MessageHeaderReceived;
            if (messageHeaderReceived != null)
            {
                messageHeaderReceived(this, message);
            }
        }

        protected void OnMessageRecived(ImapMessage message)
        {
            MessageReceivedEventHandler messageReceived = this.MessageReceived;
            if (messageReceived != null)
            {
                messageReceived(this, message);
            }
        }

        protected virtual void OnQuit()
        {
            ClientEventHandler quit = this.Quit;
            if (quit != null)
            {
                quit(this);
            }
        }

        protected virtual void OnRecentChanged()
        {
            StateChangedEventHandler recentChanged = this.RecentChanged;
            if (recentChanged != null)
            {
                recentChanged(this, this._activeMailbox);
            }
        }

        protected void OnServerResponseReceived(IMAP4Response response)
        {
            Skycap.Net.Imap.Events.ServerResponseReceived serverResponseReceived = this.ServerResponseReceived;
            if (serverResponseReceived != null)
            {
                serverResponseReceived(this, response);
            }
        }

        public virtual CompletionResponse RenameMailbox(Mailbox mailbox, string name)
        {
            CompletionResponse response2;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Mailbox name can't be null or empty", "name");
            }
            if (name.Contains("/"))
            {
                throw new FormatException("Mailbox name can't contains hierarchy delimiter symbol");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                this.SelectInbox();
                string str = StringEncoding.EncodeMailboxName(name);
                string newName = string.IsNullOrEmpty(mailbox.Parent.FullName) ? str : string.Format("{0}{1}{2}", mailbox.Parent.FullName, mailbox.HierarchyDelimiter, str);
                CompletionResponse response = new RENAMECommand(mailbox.FullName, newName).Interact(this._dispatcher);
                this.OnCompleted();
                response2 = response;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response2;
        }

        public virtual IEnumerable<uint> Search(Query query, Mailbox mailbox)
        {
            IEnumerable<uint> enumerable;
            if (query == null)
            {
                throw new ArgumentNullException("query", "Query can't be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                UIDSEARCHCommand command = new UIDSEARCHCommand(null, query);
                command.Interact(this._dispatcher);
                uint[] numArray = new uint[command.Uids.Count];
                for (int i = 0; i < numArray.Length; i++)
                {
                    numArray[i] = command.Uids[i].ToUint();
                }
                enumerable = numArray;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return enumerable;
        }

        public virtual CompletionResponse Select(Mailbox mailbox)
        {
            CompletionResponse response2;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox cannot be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                CompletionResponse response = new SELECTCommand(mailbox.FullName).Interact(this._dispatcher);
                if (response.CompletionResult == ECompletionResponseType.OK)
                {
                    this._state |= EClientState.Selected;
                    this._activeMailbox = mailbox;
                }
                response2 = response;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response2;
        }

        public CompletionResponse Append(Mailbox mailbox, StructuredMessage message, out string uid)
        {
            CompletionResponse response2;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "Message can't be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                SmtpMessage smtpMessage = new SmtpMessage(message, Encoding.UTF8);
                IEnumerable<string> mimeData = MailMessageRFCEncoder.GetEncoded(smtpMessage);
                CompletionResponse response = new APPENDCommand(mailbox.FullName, string.Join(Environment.NewLine, mimeData)).Interact(this._dispatcher);
                Match match = AppendUidRegex.Match(response.Response);
                uid = match.Groups[2].Value;
                response2 = response;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response2;
        }

        protected virtual CompletionResponse SelectInbox()
        {
            this.CheckState(EClientState.Loggined);
            CompletionResponse response = new SELECTCommand("INBOX").Interact(this._dispatcher);
            this._state |= EClientState.Selected;
            return response;
        }

        public virtual CompletionResponse SetMessageFlags(ISequence uidSequence, Mailbox mailbox, MessageFlagCollection flags, EFlagMode mode)
        {
            CompletionResponse response3;
            if (uidSequence == null)
            {
                throw new ArgumentNullException("uidSequence", "Message sequence cannot be null");
            }
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            if (flags == null)
            {
                throw new ArgumentNullException("flags", "Flags can't be null");
            }
            this.CheckState(EClientState.Loggined);
            try
            {
                if (this.Select(mailbox).CompletionResult != ECompletionResponseType.OK)
                {
                    throw new BadMailboxException("Mailbox doesn't exist or you can't access it");
                }
                CompletionResponse response2 = new UIDSTORECommand(uidSequence, flags, mode).Interact(this._dispatcher);
                this.OnCompleted();
                response3 = response2;
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response3;
        }

        public virtual CompletionResponse Subscribe(Mailbox mailbox)
        {
            CompletionResponse response;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            try
            {
                response = new SUBSCRIBECommand(mailbox.FullName).Interact(this._dispatcher);
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response;
        }

        public virtual CompletionResponse Unsubscribe(Mailbox mailbox)
        {
            CompletionResponse response;
            if (mailbox == null)
            {
                throw new ArgumentNullException("mailbox", "Mailbox can't be null");
            }
            try
            {
                response = new UNSUBSCRIBECommand(mailbox.FullName).Interact(this._dispatcher);
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            return response;
        }

        private Dictionary<string, string> GetCopyResponse(string response)
        {
            Dictionary<string, string> newUids = new Dictionary<string, string>();
            Regex regNewId = new Regex(@"\[COPYUID \d+ (.+) (.+)] \(Success\)", RegexOptions.IgnoreCase);
            GroupCollection groups = regNewId.Match(response).Groups;
            if (groups.Count == 3)
            {
                string[] old = groups[1].Value.Split(',', ':');
                string[] @new = groups[2].Value.Split(',', ':');
                for (int i = 0; i < old.Length; i++)
                {
                    newUids.Add(old[i], @new[i]);
                }
            }
            return newUids;
        }

        public virtual string AttachmentDirectory
        {
            get
            {
                return this._configurationProvider.AttachmentDirectory;
            }
            set
            {
                this._configurationProvider.AttachmentDirectory = value;
            }
        }

        public virtual EAuthenticationType AuthenticationType
        {
            get
            {
                return this._configurationProvider.AuthenticationType;
            }
            set
            {
                this._configurationProvider.AuthenticationType = value;
            }
        }

        public virtual string Host
        {
            get
            {
                return this._configurationProvider.Host;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.Host = value;
            }
        }

        public virtual string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._password = value;
            }
        }

        public virtual ushort Port
        {
            get
            {
                return this._configurationProvider.Port;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.Port = value;
            }
        }

        public virtual string ProxyHost
        {
            get
            {
                return this._configurationProvider.ProxyHost;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.ProxyHost = value;
            }
        }

        public virtual string ProxyPassword
        {
            get
            {
                return this._configurationProvider.ProxyPassword;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.ProxyPassword = value;
            }
        }

        public virtual ushort ProxyPort
        {
            get
            {
                return this._configurationProvider.ProxyPort;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.ProxyPort = value;
            }
        }

        public virtual EProxyType ProxyType
        {
            get
            {
                return this._configurationProvider.ProxyType;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.ProxyType = value;
            }
        }

        public virtual string ProxyUser
        {
            get
            {
                return this._configurationProvider.ProxyUser;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.ProxyUser = value;
            }
        }

        public virtual int ReceiveTimeout
        {
            get
            {
                return this._configurationProvider.ReceiveTimeOut;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.ReceiveTimeOut = value;
            }
        }

        public virtual int SendTimeout
        {
            get
            {
                return this._configurationProvider.SendTimeOut;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.SendTimeOut = value;
            }
        }

        public virtual EInteractionType SSLInteractionType
        {
            get
            {
                return this._configurationProvider.SSLInteractionType;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._configurationProvider.SSLInteractionType = value;
            }
        }

        public string AccountName
        {
            get;
            set;
        }

        public Mailbox Mailbox
        {
            get;
            set;
        }

        public virtual string Username
        {
            get
            {
                return this._login;
            }
            set
            {
                this.CheckState(EClientState.Disconnected);
                this._login = value;
            }
        }

        public virtual EClientState State
        {
            get
            {
                return this._state;
            }
        }

        private delegate CompletionResponse CopyDelegate(ISequence uidSequence, Mailbox source, Mailbox destination);

        private delegate CompletionResponse CreateMailbox2Delegate(string folderName, Mailbox rootMailbox);

        private delegate CompletionResponse CreateMailboxDelegate(string folderName);

        private delegate CompletionResponse DeleteMailboxDelegate(Mailbox mailbox);

        private delegate CompletionResponse DeleteMarkedMessagesDelegate(Mailbox mailbox);

        private delegate MessageCollection GetAllMesssageDelegate(Mailbox mailbox);

        private delegate Attachment GetAttachmentDelegate(Mailbox mailbox, ImapMessage message, Attachment attachment);

        private delegate MailboxCollection GetMailboxCollectionDelegate();

        private delegate Mailbox GetMailboxTreeDelegate();

        private delegate uint GetMessageCountDelegate(Mailbox mailbox);

        private delegate MessageCollection GetMessagesHeader2Delegate(Mailbox mailbox, uint start, uint end);

        private delegate MessageCollection GetMessagesHeaderDelegate(Mailbox mailbox, IEnumerable<uint> uids);

        private delegate ImapMessage GetMesssageDelegate(uint id, Mailbox mailbox);

        private delegate ImapMessage GetMesssageTextDelegate(uint uid, Mailbox mailbox);

        private delegate MailboxCollection GetSubscribedMailboxCollectionDelegate();

        private delegate Mailbox GetSubscribedMailboxTreeDelegate();

        private delegate CompletionResponse LoginDelegate();

        private delegate CompletionResponse LogoutDelegate();

        private delegate CompletionResponse MarkAsDeletedDelegate(ISequence uidSequence, Mailbox mailbox);

        private delegate CompletionResponse MarkAsSeenDelegate(ISequence uidSequence, Mailbox mailbox);

        private delegate CompletionResponse MarkAsUndeletedDelegate(ISequence uidSequence, Mailbox mailbox);

        private delegate CompletionResponse MarkAsUnseenDelegate(ISequence uidSequence, Mailbox mailbox);

        private delegate CompletionResponse NoopDelegate();

        private delegate CompletionResponse RenameDelegate(Mailbox mailbox, string name);

        private delegate IEnumerable<uint> SearchDelegate(Query query, Mailbox name);

        private delegate CompletionResponse SetMessageFlagsDelegate(ISequence uidSequence, Mailbox mailbox, MessageFlagCollection flags, EFlagMode mode);

        private delegate CompletionResponse SubscribeDelegate(Mailbox mailbox);

        private delegate CompletionResponse UnsubscribeDelegate(Mailbox mailbox);
    }
}

