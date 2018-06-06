namespace Skycap.Net.Smtp
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Collections;
    using Skycap.Net.Common.Configurations;
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Common.Exceptions;
    using Skycap.Net.Smtp.ServerActions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class SmtpClient
    {
        protected CodeConfigurationProvider _configurationProvider;
        protected IConnectionFactory _connectionFactory;
        protected string _password;
        protected ESmtpClientState _state;
        protected string _username;
        public const string ConnectionShouldBeOpenedMessage = "Connection should be opened";
        public const string FromFieldNotSpecifiedMessage = "Field From should be specified";
        public const string IncorrectPlainTextInitializationMessage = "HTML content type is specified, but field PlainText is not initialized";
        public const string NeitherMessageCanBeNull = "Neither message can be null in the messages collection";
        public const string NoTextSpecifiedMessage = "No Text specified for the message";
        public const string NoToFieldSpecifiedMessage = "The To field is not specified for the message";
        public const string OnlyPlainOrHtmlSupportedMessage = "Field Text can be only in plain or html format";
        public const string OtherThreadIsActive = "SmtpClient doesn't allow sending emails in separate threads using one object";
        public const string WrongAuthenticationMethodSelectedMessage = "Server doesn't support existed authentication mechanisms or doesn't require any authentication";

        public event SmtpClientEventHandler Authenticated;

        public event SmtpClientEventHandler Completed;

        public event SmtpClientEventHandler Connected;

        public event SmtpClientEventHandler MessageSent;

        public SmtpClient()
        {
            this.Init("localhost", 0x19, "username", "password", EInteractionType.Plain, EAuthenticationType.Auto);
        }

        public SmtpClient(CodeConfigurationProvider configurationProvider, string smtpUser, string smtpPassword)
        {
            if (configurationProvider == null)
            {
                throw new ArgumentNullException("configurationProvider");
            }
            if (smtpUser == null)
            {
                throw new ArgumentNullException("smtpUser");
            }
            if (smtpPassword == null)
            {
                throw new ArgumentNullException("smtpPassword");
            }
            this._configurationProvider = configurationProvider;
            this._connectionFactory = new ConnectionFactory();
            this._username = smtpUser;
            this._password = smtpPassword;
        }

        public SmtpClient(string host, ushort port, EInteractionType interactionType, EAuthenticationType authenticationType)
        {
            this.Init(host, port, "", "", interactionType, authenticationType);
        }

        public SmtpClient(string host, ushort port, string smtpUser, string smtpPassword)
        {
            this.Init(host, port, smtpUser, smtpPassword, EInteractionType.Plain, EAuthenticationType.Auto);
        }

        public SmtpClient(string host, ushort port, string smtpUser, string smtpPassword, EInteractionType interactionType, EAuthenticationType authenticationType)
        {
            this.Init(host, port, smtpUser, smtpPassword, interactionType, authenticationType);
        }

        public virtual IAsyncResult BeginSendOne(SmtpMessage message, AsyncCallback callback)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            List<SmtpMessage> list = new List<SmtpMessage> {
                message
            };
            SendOneDelegate delegate2 = new SendOneDelegate(this.SendOne);
            return delegate2.BeginInvoke(message, callback, this);
        }

        public virtual IAsyncResult BeginSendSome(IEnumerable<SmtpMessage> messages, AsyncCallback callback)
        {
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }
            SendSomeDelegate delegate2 = new SendSomeDelegate(this.SendSome);
            return delegate2.BeginInvoke(messages, callback, this);
        }

        protected void Dispose(bool disposing)
        {

        }

        protected virtual SmtpResponse DoAction(ISmtpAction action, IConnection connection)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            return action.Interact(connection);
        }

        protected static ISmtpAction GetAuthenticationCommand(string user, string password, EAuthenticationType authenticationType, IEnumerable<SmtpResponseLine> serverExtensions)
        {
            switch (authenticationType)
            {
                case EAuthenticationType.Auto:
                    foreach (SmtpResponseLine line in serverExtensions)
                    {
                        if (line.Comment.Contains("AUTH"))
                        {
                            if (line.Comment.Contains("PLAIN"))
                            {
                                return new AUTHPlainCommand(user, password);
                            }
                            if (line.Comment.Contains("LOGIN"))
                            {
                                return new AUTHLoginCommand(user, password);
                            }
                        }
                    }
                    return null;

                case EAuthenticationType.Plain:
                    if (!IsAuthMethosSupported(serverExtensions, "PLAIN"))
                    {
                        throw new AuthenticationMethodNotSupportedException();
                    }
                    return new AUTHPlainCommand(user, password);

                case EAuthenticationType.Login:
                    if (!IsAuthMethosSupported(serverExtensions, "LOGIN"))
                    {
                        throw new AuthenticationMethodNotSupportedException();
                    }
                    return new AUTHLoginCommand(user, password);

                default:
                    return null;
            }
        }

        protected void Init(string host, ushort port, string smtpUser, string smtpPassword, EInteractionType interactionType, EAuthenticationType authenticationType)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (smtpUser == null)
            {
                throw new ArgumentNullException("smtpUser");
            }
            if (smtpPassword == null)
            {
                throw new ArgumentNullException("smtpPassword");
            }
            this._configurationProvider = new CodeConfigurationProvider(host, port, interactionType, authenticationType);
            this._connectionFactory = new ConnectionFactory();
            this.Username = smtpUser;
            this.Password = smtpPassword;
            this.ProxyUser = "";
            this.State = ESmtpClientState.Awaiting;
        }

        protected static bool IsAuthMethosSupported(IEnumerable<SmtpResponseLine> serverExtensions, string authMethod)
        {
            foreach (SmtpResponseLine line in serverExtensions)
            {
                if (line.Comment.Contains("AUTH") && line.Comment.Contains(authMethod))
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual IList<SendResult> PerformSendMailInteraction(IEnumerable<SmtpMessage> messages, IConnection connection)
        {
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }
            if (connection.State != EConnectionState.Connected)
            {
                throw new InvalidOperationException("Connection should be opened");
            }
            IList<SendResult> list = new List<SendResult>();
            if (this.Connected != null)
            {
                this.Connected(this);
            }
            SmtpResponse lastResponse = new SmtpResponse(connection);
            if (lastResponse.Type != ESmtpResponseType.PositiveCompletion)
            {
                list.Add(new SendResult(ESendResult.StrangeServerResponse, lastResponse));
                return list;
            }
            if (this.DoAction(new EHLOCommand(), connection).Type != ESmtpResponseType.PositiveCompletion)
            {
                SmtpResponse response3 = this.DoAction(new HELOCommand(), connection);
                if (response3.Type != ESmtpResponseType.PositiveCompletion)
                {
                    list.Add(new SendResult(ESendResult.StrangeServerResponse, response3));
                    return list;
                }
            }
            if (this._configurationProvider.SSLInteractionType == EInteractionType.StartTLS)
            {
                SmtpResponse response4 = this.DoAction(new StartTlsCommand(), connection);
                if (response4.Type != ESmtpResponseType.PositiveCompletion)
                {
                    list.Add(new SendResult(ESendResult.SslWasNotStarted, response4));
                    return list;
                }
                connection.SwitchToSslChannel();
            }
            SmtpResponse response2 = this.DoAction(new EHLOCommand(), connection);
            if (response2.Type != ESmtpResponseType.PositiveCompletion)
            {
                SmtpResponse response5 = this.DoAction(new HELOCommand(), connection);
                if (response5.Type != ESmtpResponseType.PositiveCompletion)
                {
                    list.Add(new SendResult(ESendResult.StrangeServerResponse, response5));
                    return list;
                }
            }
            if (this._configurationProvider.AuthenticationType != EAuthenticationType.None)
            {
                ISmtpAction action = GetAuthenticationCommand(this.Username, this.Password, this._configurationProvider.AuthenticationType, response2.Lines);
                if (action == null)
                {
                    throw new InvalidOperationException("Server doesn't support existed authentication mechanisms or doesn't require any authentication");
                }
                SmtpResponse response6 = this.DoAction(action, connection) as AuthCommandResponse;
                if (response6.Type != ESmtpResponseType.PositiveCompletion)
                {
                    SendResult item = new SendResult(ESendResult.AuthenticationFailed, response6);
                    try
                    {
                        this.DoAction(new QUITCommand(), connection);
                    }
                    catch { }
                    list.Add(item);
                    return list;
                }
            }
            if (this.Authenticated != null)
            {
                this.Authenticated(this);
            }
            List<EmailAddress> invalidEmails = new List<EmailAddress>();
            foreach (SmtpMessage message in messages)
            {
                SmtpResponse response7 = this.DoAction(new MAILCommand(message.From), connection);
                if (response7.Type != ESmtpResponseType.PositiveCompletion)
                {
                    list.Add(new SendResult(ESendResult.FromAddressFailed, response7));
                }
                else
                {
                    foreach (EmailAddress address in message.To)
                    {
                        if (this.DoAction(new RCPTCommand(address), connection).Type != ESmtpResponseType.PositiveCompletion)
                        {
                            invalidEmails.Add(address);
                        }
                    }
                    foreach (EmailAddress address2 in message.CarbonCopies)
                    {
                        if (this.DoAction(new RCPTCommand(address2), connection).Type != ESmtpResponseType.PositiveCompletion)
                        {
                            invalidEmails.Add(address2);
                        }
                    }
                    foreach (EmailAddress address3 in message.BlindedCarbonCopies)
                    {
                        if (this.DoAction(new RCPTCommand(address3), connection).Type != ESmtpResponseType.PositiveCompletion)
                        {
                            invalidEmails.Add(address3);
                        }
                    }
                    IEnumerable<string> encoded = MailMessageRFCEncoder.GetEncoded(message);
                    SmtpResponse response11 = this.DoAction(new DATACommand(encoded), connection);
                    if (response11.Type != ESmtpResponseType.PositiveCompletion)
                    {
                        list.Add(new SendResult(ESendResult.DataSendingFailed, response11, invalidEmails));
                        continue;
                    }
                    if (this.MessageSent != null)
                    {
                        this.MessageSent(this);
                    }
                    if (invalidEmails.Count == 0)
                    {
                        list.Add(new SendResult(ESendResult.Ok, response11));
                        continue;
                    }
                    list.Add(new SendResult(ESendResult.OkWithInvalidEmails, response11, invalidEmails));
                }
            }
            try
            {
                this.DoAction(new QUITCommand(), connection);
            }
            catch { }
            if (this.Completed != null)
            {
                this.Completed(this);
            }
            return list;
        }

        public virtual SendResult SendOne(SmtpMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            List<SmtpMessage> messages = new List<SmtpMessage> {
                message
            };
            return this.SendSome(messages)[0];
        }

        public virtual SendResult SendOne(string addressFrom, IEnumerable<string> addressTo, string messageSubject, string messageText)
        {
            EmailAddress from = new EmailAddress(addressFrom);
            EmailAddressCollection to = new EmailAddressCollection();
            foreach (string str in addressTo)
            {
                to.Add(new EmailAddress(str));
            }
            SmtpMessage message = new SmtpMessage(from, to, messageSubject, messageText);
            return this.SendOne(message);
        }

        public virtual SendResult SendOne(string addressFrom, string addressTo, string messageSubject, string messageText)
        {
            EmailAddress from = new EmailAddress(addressFrom);
            EmailAddress to = new EmailAddress(addressTo);
            SmtpMessage message = new SmtpMessage(from, to, messageSubject, messageText);
            return this.SendOne(message);
        }

        public virtual IList<SendResult> SendSome(IEnumerable<SmtpMessage> messages)
        {
            IList<SendResult> list;
            if (messages == null)
            {
                throw new ArgumentNullException("messages");
            }
            foreach (SmtpMessage message in messages)
            {
                if (message == null)
                {
                    throw new ArgumentException("Neither message can be null in the messages collection");
                }
                if (message.CheckCorrectness() == EMessageCheckResult.NoPlainTextInHTMLMessage)
                {
                    throw new InvalidOperationException("HTML content type is specified, but field PlainText is not initialized");
                }
                if ((message.TextContentType != ETextContentType.Html) && (message.TextContentType != ETextContentType.Plain))
                {
                    throw new NotSupportedException("Field Text can be only in plain or html format");
                }
                if (message.CheckCorrectness() == EMessageCheckResult.NoFromField)
                {
                    throw new InvalidOperationException("Field From should be specified");
                }
                if (message.CheckCorrectness() == EMessageCheckResult.NoText)
                {
                    throw new InvalidOperationException("No Text specified for the message");
                }
                if (message.CheckCorrectness() == EMessageCheckResult.NoToField)
                {
                    throw new InvalidOperationException("The To field is not specified for the message");
                }
            }
            lock (this)
            {
                if (this.State != ESmtpClientState.Awaiting)
                {
                    throw new InvalidOperationException("SmtpClient doesn't allow sending emails in separate threads using one object");
                }
                this.State = ESmtpClientState.Sending;
            }
            IConnection connection = this._connectionFactory.GetConnection(this._configurationProvider);
            try
            {
                connection.Open();
                list = this.PerformSendMailInteraction(messages, connection);
            }
            catch (Exception)
            {
                lock (this)
                {
                    this.State = ESmtpClientState.Awaiting;
                }
                throw;
            }
            finally
            {
                connection.Close();
            }
            this.State = ESmtpClientState.Awaiting;
            return list;
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
                this._configurationProvider.ProxyUser = value;
            }
        }

        public virtual int ReceiveTimeOut
        {
            get
            {
                return this._configurationProvider.ReceiveTimeOut;
            }
            set
            {
                this._configurationProvider.ReceiveTimeOut = value;
            }
        }

        public virtual int SendTimeOut
        {
            get
            {
                return this._configurationProvider.SendTimeOut;
            }
            set
            {
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
                this._configurationProvider.SSLInteractionType = value;
            }
        }

        public virtual ESmtpClientState State
        {
            get
            {
                return this._state;
            }
            protected set
            {
                this._state = value;
            }
        }

        public virtual string Username
        {
            get
            {
                return this._username;
            }
            set
            {
                this._username = value;
            }
        }

        protected delegate SendResult SendOneDelegate(SmtpMessage message);

        protected delegate IList<SendResult> SendSomeDelegate(IEnumerable<SmtpMessage> messages);
    }
}

