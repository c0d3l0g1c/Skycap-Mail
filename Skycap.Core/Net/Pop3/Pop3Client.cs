namespace Skycap.Net.Pop3
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Collections;
    using Skycap.Net.Common.Configurations;
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Common.Exceptions;
    using Skycap.Net.Pop3.Command;
    using Skycap.Net.Pop3.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Windows.Storage;

    public class Pop3Client
    {
        protected IConfigurationProvider _configurationProvider;
        protected IConnection _connection;
        protected IConnectionFactory _connectionFactory;
        internal EPop3ConnectionState _connectionState;
        protected string _password;
        internal EPop3ClientState _state;
        protected string _username;
        public const string exAuthenticationMethodNotSupported = "Selected authentication method is not supported by the server";
        public const string exOtherThreadIsActive = "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object";
        public const string exServerDontKnowAPOP = "Server doesnt support the APOP command";
        public const string exWrongStatus = "The command cannot be executed in this state";

        public event Pop3ClientEventHandler Authenticated;

        public event Pop3MessageInfoEventHandler BrokenMessage;

        public event Pop3ClientEventHandler Completed;

        public event Pop3ClientEventHandler Connected;

        public event Pop3ClientEventHandler Disconnected;

        public event Pop3MessageIDHandler MessageDeleted;

        public event Pop3MessageEventHandler MessageReceived;

        public event Pop3ClientEventHandler Quit;

        public Pop3Client()
        {
            this._configurationProvider = new CodeConfigurationProvider("localhost", 0x19);
            this._connectionFactory = new ConnectionFactory();
            this.Username = "";
            this.Password = "";
            this.ConnectionState = EPop3ConnectionState.Disconnected;
            this.State = EPop3ClientState.Awaiting;
            this.AuthenticationType = EAuthenticationType.Auto;
            this.ProxyUser = "";
        }

        public Pop3Client(IConfigurationProvider configurationProvider, string login, string password, EAuthenticationType authenticationType)
        {
            if (configurationProvider == null)
            {
                throw new ArgumentNullException("configurationProvider");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            this._configurationProvider = configurationProvider;
            this._connectionFactory = new ConnectionFactory();
            this.Username = login;
            this.Password = password;
            this.ConnectionState = EPop3ConnectionState.Disconnected;
            this.State = EPop3ClientState.Awaiting;
            this.AuthenticationType = authenticationType;
        }

        public Pop3Client(string accountName, string host, ushort port, EInteractionType interactionType, EAuthenticationType authenticationType)
        {
            this.Init(accountName, host, port, "", "", interactionType, authenticationType);
        }

        public Pop3Client(string accountName, string host, ushort port, string login, string password)
        {
            this.Init(accountName, host, port, login, password, EInteractionType.Plain, EAuthenticationType.Auto);
        }

        public Pop3Client(string accountName, string host, ushort port, string login, string password, EInteractionType interactionType, EAuthenticationType authenticationType)
        {
            this.Init(accountName, host, port, login, password, interactionType, authenticationType);
        }

        protected virtual Rfc822MessageCollection _GetAllMessages()
        {
            Pop3MessageUIDInfoCollection infos;
            string str;
            Rfc822MessageCollection messages = new Rfc822MessageCollection();
            if (!this._TryGetAllUIDMessages(out infos, out str))
            {
                throw new Pop3ReceiveException(str);
            }
            for (int i = 0; i < infos.Count; i++)
            {
                PopMessage message;
                string str2;
                Pop3MessageUIDInfo info = infos[i];
                if (this._TryGetMessage(info.SerialNumber, info.UniqueNumber, out message, out str2))
                {
                    messages.Add(message);
                }
                else if (this.BrokenMessage == null)
                {
                    throw new Pop3ReceiveMessageException(str2, message);
                }
            }
            if (this.Completed != null)
            {
                this.Completed(this);
            }
            this.State = EPop3ClientState.Awaiting;
            return messages;
        }

        protected virtual Rfc822MessageCollection _GetMessages(IEnumerable<uint> serialNumbers, IEnumerable<string> uids)
        {
            Rfc822MessageCollection messages = new Rfc822MessageCollection();
            IList<string> uidsList = uids.ToList();
            int i = 0;
            foreach (uint num in serialNumbers)
            {
                PopMessage message;
                string str;
                if (this._TryGetMessage(num, uidsList[i], out message, out str))
                {
                    messages.Add(message);
                }
                else if (this.BrokenMessage == null)
                {
                    throw new Pop3ReceiveMessageException(str, message);
                }
                i++;
            }
            if (this.Completed != null)
            {
                this.Completed(this);
            }
            return messages;
        }

        protected virtual bool _TryDeleteMessage(uint serialNumber, out Pop3Response responce)
        {
            DELE command = new DELE(serialNumber);
            Pop3Response response = this.DoCommand(command);
            if (response.Type == EPop3ResponseType.OK)
            {
                if (this.MessageDeleted != null)
                {
                    this.MessageDeleted(this, serialNumber);
                }
                responce = new Pop3Response();
                return true;
            }
            responce = new Pop3Response(response.Message);
            return false;
        }

        protected virtual bool _TryGetAllUIDMessages(out Pop3MessageUIDInfoCollection messageUIDInfo, out string errorMessage)
        {
            UIDL command = new UIDL();
            Pop3Response response = this.DoCommand(command);
            errorMessage = "";
            if (response.Type == EPop3ResponseType.OK)
            {
                messageUIDInfo = command.Messages;
                return true;
            }
            messageUIDInfo = null;
            errorMessage = response.Message;
            return false;
        }

        protected virtual bool _TryGetMessage(uint number, string uid, out PopMessage message, out string errorMessage)
        {
            RETR command = new RETR(number, uid, this._configurationProvider.AttachmentDirectory);
            Pop3Response response = this.DoCommand(command);
            if (response.Type == EPop3ResponseType.OK)
            {
                if (this.MessageReceived != null)
                {
                    this.MessageReceived(this, command.Message);
                }
                errorMessage = "";
                message = command.Message;
                return true;
            }
            if (this.BrokenMessage != null)
            {
                this.BrokenMessage(this, new Pop3MessageInfo(number, 0), response.Message, command.Message);
            }
            errorMessage = response.Message;
            message = command.Message;
            return false;
        }

        protected virtual bool _TryGetMessageInfo(uint number, out Pop3MessageInfo messageInfo, out string errorMessage)
        {
            errorMessage = "";
            LIST command = new LIST(number);
            Pop3Response response = this.DoCommand(command);
            if (command.Messages.Count > 0)
            {
                messageInfo = command.Messages[0];
            }
            else
            {
                errorMessage = response.Message;
                messageInfo = null;
            }
            return (response.Type == EPop3ResponseType.OK);
        }

        protected virtual bool _TryGetMessagesInfo(out Pop3MessageInfoCollection infoCollection, out string errorMessage)
        {
            infoCollection = new Pop3MessageInfoCollection();
            errorMessage = "";
            LIST command = new LIST();
            Pop3Response response = this.DoCommand(command);
            if (response.Type == EPop3ResponseType.OK)
            {
                infoCollection = command.Messages;
            }
            else
            {
                errorMessage = response.Message;
            }
            return true;
        }

        private bool _TryGetStatistic(out Pop3MessageStatistics statistic, out string errorMessage)
        {
            STAT command = new STAT();
            Pop3Response response = this.DoCommand(command);
            errorMessage = "";
            statistic = command.Statistics;
            if (response.Type == EPop3ResponseType.ERR)
            {
                errorMessage = response.Message;
                return false;
            }
            return true;
        }

        protected virtual bool _TryGetUIDMessage(uint serialNumber, out Pop3MessageUIDInfo messageUIDInfo, out string errorMessage)
        {
            UIDL command = new UIDL(serialNumber);
            Pop3Response response = this.DoCommand(command);
            errorMessage = "";
            if (command.Messages.Count > 0)
            {
                messageUIDInfo = command.Messages[0];
                return true;
            }
            messageUIDInfo = null;
            errorMessage = response.Message;
            return false;
        }

        protected virtual bool _TryNoop(out Pop3Response responce)
        {
            responce = this.DoCommand(new NOOP());
            if (responce.Type == EPop3ResponseType.ERR)
            {
                return false;
            }
            return true;
        }

        protected virtual bool _TryReset(out Pop3Response responce)
        {
            responce = this.DoCommand(new RSET());
            if (responce.Type == EPop3ResponseType.ERR)
            {
                return false;
            }
            return true;
        }

        public virtual IAsyncResult BeginRecv(AsyncCallback callback)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            RecvAllDelegate delegate2 = new RecvAllDelegate(this._GetAllMessages);
            return delegate2.BeginInvoke(callback, this);
        }

        public virtual IAsyncResult BeginRecv(IEnumerable<uint> serialNumbers, IEnumerable<string> uids, AsyncCallback callback)
        {
            if (serialNumbers == null)
            {
                throw new ArgumentNullException("serialNumbers");
            }
            if (uids == null)
            {
                throw new ArgumentNullException("uids");
            }
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            RecvDelegate delegate2 = new RecvDelegate(this._GetMessages);
            return delegate2.BeginInvoke(serialNumbers, uids, callback, this);
        }

        public virtual Pop3Response DeleteMessage(uint serialNumber)
        {
            Pop3Response response;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            this._TryDeleteMessage(serialNumber, out response);
            this.State = EPop3ClientState.Awaiting;
            return response;
        }

        protected void Dispose(bool disposing)
        {

        }

        protected virtual Pop3Response DoCommand(IPOP3Command command)
        {
            Pop3Response response;
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            try
            {
                response = command.Interact(this._connection);
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                if (exception is IOException)
                {
                    if (Login().Type == EPop3ResponseType.OK)
                        return DoCommand(command);
                }
                throw;
            }
            return response;
        }

        public virtual void EndRecv(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            IAsyncResult result = (IAsyncResult) asyncResult;
            RecvAllDelegate asyncDelegate = result.AsyncState as RecvAllDelegate;
            if (asyncDelegate != null)
            {
                asyncDelegate.EndInvoke(asyncResult);
                this.State = EPop3ClientState.Awaiting;
            }
            else
            {
                RecvDelegate delegate3 = result.AsyncState as RecvDelegate;
                if (delegate3 != null)
                {
                    delegate3.EndInvoke(asyncResult);
                    this.State = EPop3ClientState.Awaiting;
                }
                else
                {
                    this.State = EPop3ClientState.Awaiting;
                }
            }
        }

        public virtual Rfc822MessageCollection GetAllMessages()
        {
            Pop3MessageUIDInfoCollection infos;
            string str;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            Rfc822MessageCollection messages = new Rfc822MessageCollection();
            if (this._TryGetAllUIDMessages(out infos, out str))
            {
                for (int i = infos.Count - 1; i > 0; i--)
                {
                    PopMessage message;
                    string str2;
                    Pop3MessageUIDInfo info = infos[i];
                    if (this._TryGetMessage(info.SerialNumber, info.UniqueNumber, out message, out str2))
                    {
                        messages.Add(message);
                    }
                    else if (this.BrokenMessage == null)
                    {
                        this.State = EPop3ClientState.Awaiting;
                        throw new Pop3ReceiveMessageException(str2, message);
                    }
                }
            }
            else
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3ReceiveException(str);
            }
            if (this.Completed != null)
            {
                this.Completed(this);
            }
            this.State = EPop3ClientState.Awaiting;
            return messages;
        }

        public virtual Pop3MessageUIDInfoCollection GetAllUIDMessages()
        {
            Pop3MessageUIDInfoCollection infos;
            string str;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            if (this._TryGetAllUIDMessages(out infos, out str))
            {
                this.State = EPop3ClientState.Awaiting;
                return infos;
            }
            this.State = EPop3ClientState.Awaiting;
            throw new Pop3ReceiveException(str);
        }

        public virtual PopMessage GetMessage(uint number, string uid)
        {
            PopMessage message;
            string str;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            if (this._TryGetMessage(number, uid, out message, out str))
            {
                this.State = EPop3ClientState.Awaiting;
                if (this.Completed != null)
                {
                    this.Completed(this);
                }
                return message;
            }
            this.State = EPop3ClientState.Awaiting;
            if (this.Completed != null)
            {
                this.Completed(this);
            }
            throw new Pop3ReceiveMessageException(str, message);
        }

        public virtual Pop3MessageInfo GetMessageInfo(uint number)
        {
            Pop3MessageInfo info;
            string str;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            if (this._TryGetMessageInfo(number, out info, out str))
            {
                this.State = EPop3ClientState.Awaiting;
                return info;
            }
            this.State = EPop3ClientState.Awaiting;
            throw new Pop3ReceiveException(str);
        }

        public virtual Pop3MessageInfoCollection GetMessagesInfo()
        {
            Pop3MessageInfoCollection infos;
            string str;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                lock (this)
                {
                    this.State = EPop3ClientState.Awaiting;
                }
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            if (this._TryGetMessagesInfo(out infos, out str))
            {
                lock (this)
                {
                    this.State = EPop3ClientState.Awaiting;
                }
                return infos;
            }
            this.State = EPop3ClientState.Awaiting;
            throw new Pop3ReceiveException(str);
        }

        public virtual Pop3MessageStatistics GetStatistic()
        {
            string str;
            Pop3MessageStatistics statistics;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            if (!this._TryGetStatistic(out statistics, out str))
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3ReceiveException(str);
            }
            this.State = EPop3ClientState.Awaiting;
            return statistics;
        }

        public virtual Pop3MessageUIDInfo GetUIDMessage(uint serialNumber)
        {
            Pop3MessageUIDInfo info;
            string str;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            if (this._TryGetUIDMessage(serialNumber, out info, out str))
            {
                this.State = EPop3ClientState.Awaiting;
                return info;
            }
            this.State = EPop3ClientState.Awaiting;
            throw new Pop3ReceiveException(str);
        }

        protected void HandleException(Exception exception)
        {
            if (exception is IOException)
            {
                this.State = EPop3ClientState.Awaiting;
                this._connection.Close();
            }
            else if (exception is ConnectionException)
            {
                this.State = EPop3ClientState.Awaiting;
                this._connection.Close();
            }
            else if (exception is Exception)
            {
                this.State = EPop3ClientState.Awaiting;
                this._connection.Close();
            }
        }

        protected virtual void Init(string accountName, string host, ushort port, string login, string password, EInteractionType interactionType, EAuthenticationType authenticationType)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                throw new ArgumentNullException("accountName");
            }
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            this._connectionFactory = new ConnectionFactory();
            this._configurationProvider = new CodeConfigurationProvider(host, port, interactionType, authenticationType);
            this.AuthenticationType = authenticationType;
            this.AccountName = accountName;
            this.Username = login;
            this.Password = password;
            this.ConnectionState = EPop3ConnectionState.Disconnected;
            this.State = EPop3ClientState.Awaiting;
        }

        public virtual Pop3Response Login()
        {
            Pop3Client client7;
            this._connection = this._connectionFactory.GetConnection(this._configurationProvider);
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState == EPop3ConnectionState.Connected)
            {
                lock (this)
                {
                    this.State = EPop3ClientState.Awaiting;
                }
                return new Pop3Response("+OK");
            }
            try
            {
                this._connection.Open();
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
                throw;
            }
            if (this._connection.State == EConnectionState.Connected)
            {
                this.ConnectionState = EPop3ConnectionState.Connected;
                if (this.Connected != null)
                {
                    this.Connected(this);
                }
            }
            Pop3Response response = new Pop3Response(this._connection.ReceiveLine());
            if (response.Type != EPop3ResponseType.OK)
            {
                goto Label_03F9;
            }
            CAPA capa = new CAPA();
            Pop3Response response2 = this.DoCommand(capa);
            if (this._configurationProvider.SSLInteractionType == EInteractionType.StartTLS)
            {
                Pop3Response response3 = this.DoCommand(new STLS());
                if (response3.Type != EPop3ResponseType.OK)
                {
                    return response3;
                }
            }
            IPOP3Command command = null;
            if ((this.AuthenticationType == EAuthenticationType.Auto) && ((response2.Type == EPop3ResponseType.ERR) || !capa.Commands.ContainsKey("SASL")))
            {
                command = new AuthStandart(this.Username, this.Password);
            }
            else
            {
                string str2 = string.Empty;
                switch (this.AuthenticationType)
                {
                    case EAuthenticationType.Auto:
                    {
                        bool flag = false;
                        if (str2.Contains("PLAIN"))
                        {
                            command = new AuthPlain(this.Username, this.Password);
                            flag = true;
                        }
                        if (str2.Contains("LOGIN"))
                        {
                            command = new AuthStandart(this.Username, this.Password);
                            flag = true;
                        }
                        if (!flag)
                        {
                            command = new AuthStandart(this.Username, this.Password);
                        }
                        goto Label_03C9;
                    }
                    case EAuthenticationType.Plain:
                        if (!str2.Contains("PLAIN"))
                        {
                            lock (this)
                            {
                                this.State = EPop3ClientState.Awaiting;
                            }
                            throw new AuthenticationMethodNotSupportedException("Selected authentication method is not supported by the server");
                        }
                        command = new AuthPlain(this.Username, this.Password);
                        goto Label_03C9;

                    case EAuthenticationType.Login:
                        if (!str2.Contains("LOGIN"))
                        {
                            lock (this)
                            {
                                this.State = EPop3ClientState.Awaiting;
                            }
                            throw new AuthenticationMethodNotSupportedException("Selected authentication method is not supported by the server");
                        }
                        command = new AuthLogin(this.Username, this.Password);
                        goto Label_03C9;
                }
            }
        Label_03C9:
            if (command != null)
            {
                response = this.DoCommand(command);
            }
            if (response.Type == EPop3ResponseType.OK)
            {
                this.ConnectionState = EPop3ConnectionState.Authenticated;
                if (this.Authenticated != null)
                {
                    this.Authenticated(this);
                }
            }
        Label_03F9:
            Monitor.Enter(client7 = this);
            try
            {
                this.State = EPop3ClientState.Awaiting;
            }
            finally
            {
                Monitor.Exit(client7);
            }
            return response;
        }

        public virtual Pop3Response Logout()
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            Pop3Response response = new Pop3Response("+OK");
            if (this.ConnectionState != EPop3ConnectionState.Disconnected)
            {
                QUIT command = new QUIT();
                response = this.DoCommand(command);
                if (this.Quit != null)
                {
                    this.Quit(this);
                }
                this.ConnectionState = EPop3ConnectionState.Disconnected;
                if (Disconnected != null) Disconnected(this);
                try
                {
                    this._connection.Close();
                }
                catch (Exception exception)
                {
                    this.HandleException(exception);
                    throw;
                }
            }
            this.State = EPop3ClientState.Awaiting;
            return response;
        }

        public Pop3Response Noop()
        {
            Pop3Response response;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            this._TryNoop(out response);
            this.State = EPop3ClientState.Awaiting;
            return response;
        }

        public virtual Pop3Response Reset()
        {
            Pop3Response response;
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    throw new InvalidOperationException("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                this.State = EPop3ClientState.Awaiting;
                throw new Pop3WrongStateException("The command cannot be executed in this state");
            }
            this._TryReset(out response);
            this.State = EPop3ClientState.Awaiting;
            return response;
        }

        public virtual bool TryDeleteMessage(uint serialNumber, out Pop3Response responce)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    responce = new Pop3Response("Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                responce = new Pop3Response("The command cannot be executed in this state");
                this.State = EPop3ClientState.Awaiting;
                return false;
            }
            bool flag = this._TryDeleteMessage(serialNumber, out responce);
            this.State = EPop3ClientState.Awaiting;
            return flag;
        }

        public virtual bool TryGetAllUIDMessages(out Pop3MessageUIDInfoCollection messageUIDInfo, out string errorMessage)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    messageUIDInfo = null;
                    errorMessage = "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object";
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                messageUIDInfo = null;
                errorMessage = "The command cannot be executed in this state";
                this.State = EPop3ClientState.Awaiting;
                return false;
            }
            bool flag = this._TryGetAllUIDMessages(out messageUIDInfo, out errorMessage);
            this.State = EPop3ClientState.Awaiting;
            return flag;
        }

        public virtual bool TryGetMessage(uint number, string uid, out PopMessage message, out string errorMessage)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    message = null;
                    errorMessage = "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object";
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                message = null;
                errorMessage = "The command cannot be executed in this state";
                this.State = EPop3ClientState.Awaiting;
                if (this.Completed != null)
                {
                    this.Completed(this);
                }
                return false;
            }
            bool flag = this._TryGetMessage(number, uid, out message, out errorMessage);
            this.State = EPop3ClientState.Awaiting;
            if (this.Completed != null)
            {
                this.Completed(this);
            }
            return flag;
        }

        public virtual bool TryGetMessageInfo(uint number, out Pop3MessageInfo messageInfo, out string errorMessage)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    messageInfo = null;
                    errorMessage = "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object";
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                messageInfo = null;
                errorMessage = "The command cannot be executed in this state";
                this.State = EPop3ClientState.Awaiting;
                return false;
            }
            bool flag = this._TryGetMessageInfo(number, out messageInfo, out errorMessage);
            this.State = EPop3ClientState.Awaiting;
            return flag;
        }

        public virtual bool TryGetMessagesInfo(out Pop3MessageInfoCollection infoCollection, out string errorMessage)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    infoCollection = null;
                    errorMessage = "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object";
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                infoCollection = null;
                errorMessage = "The command cannot be executed in this state";
                lock (this)
                {
                    this.State = EPop3ClientState.Awaiting;
                }
                return false;
            }
            bool flag = this._TryGetMessagesInfo(out infoCollection, out errorMessage);
            lock (this)
            {
                this.State = EPop3ClientState.Awaiting;
            }
            return flag;
        }

        public virtual bool TryGetStatistic(out Pop3MessageStatistics statistic, out string errorMessage)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    errorMessage = "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object";
                    statistic = new Pop3MessageStatistics();
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                errorMessage = "The command cannot be executed in this state";
                statistic = new Pop3MessageStatistics();
                this.State = EPop3ClientState.Awaiting;
                return false;
            }
            bool flag = this._TryGetStatistic(out statistic, out errorMessage);
            this.State = EPop3ClientState.Awaiting;
            return flag;
        }

        public virtual bool TryGetUIDMessage(uint serialNumber, out Pop3MessageUIDInfo messageUIDInfo, out string errorMessage)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    messageUIDInfo = null;
                    errorMessage = "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object";
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                messageUIDInfo = null;
                errorMessage = "The command cannot be executed in this state";
                this.State = EPop3ClientState.Awaiting;
                return false;
            }
            bool flag = this._TryGetUIDMessage(serialNumber, out messageUIDInfo, out errorMessage);
            this.State = EPop3ClientState.Awaiting;
            return flag;
        }

        public virtual bool TryNoop(out Pop3Response responce)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    responce = new Pop3Response(EPop3ResponseType.ERR, "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                responce = new Pop3Response(EPop3ResponseType.ERR, "The command cannot be executed in this state");
                this.State = EPop3ClientState.Awaiting;
                return false;
            }
            bool flag = this._TryNoop(out responce);
            this.State = EPop3ClientState.Awaiting;
            return flag;
        }

        public virtual bool TryReset(out Pop3Response responce)
        {
            lock (this)
            {
                if (this.State != EPop3ClientState.Awaiting)
                {
                    responce = new Pop3Response(EPop3ResponseType.ERR, "Pop3Client doesn't allow executing multiple operations simulteneously in a few threads using one object");
                    return false;
                }
                this.State = EPop3ClientState.Busy;
            }
            if (this.ConnectionState != EPop3ConnectionState.Authenticated)
            {
                responce = new Pop3Response(EPop3ResponseType.ERR, "The command cannot be executed in this state");
                this.State = EPop3ClientState.Awaiting;
                return false;
            }
            bool flag = this._TryReset(out responce);
            this.State = EPop3ClientState.Awaiting;
            return flag;
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

        public virtual EPop3ConnectionState ConnectionState
        {
            get
            {
                return this._connectionState;
            }
            protected set
            {
                this._connectionState = value;
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

        public virtual int ReceiveTimeout
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

        public virtual int SendTimeout
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

        public virtual EPop3ClientState State
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
                return this._username;
            }
            set
            {
                this._username = value;
            }
        }

        protected delegate Rfc822MessageCollection RecvAllDelegate();

        protected delegate Rfc822MessageCollection RecvDelegate(IEnumerable<uint> serialNumbers, IEnumerable<string> uids);
    }
}

