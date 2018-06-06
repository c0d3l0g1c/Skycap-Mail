namespace Skycap.Net.Imap
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Imap.Events;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
using Windows.Networking.Sockets;

    internal class InteractDispatcher : IInteractDispatcher
    {
        protected const string _cannotGetRawDataInResponseMode = "Cannot get raw data in response mode";
        protected const string _cannotGetResponseInRawMode = "Cannot get response in raw mode";
        protected Dictionary<uint, ResponseFilter> _commandFilters;
        protected IConnection _connection;
        protected bool _continueRecieve;
        protected const string _exCantSwitchToSSL = "Can't switch to ssl while receive in progress";
        protected Dictionary<string, List<uint>> _filter;
        protected EDispatcherMode _mode;
        protected uint _nextId;
        protected const string _noMonopolyCommandFind = "No monopoly command find";
        protected Dictionary<uint, Queue<string>> _queues;
        private bool _receiveInProgress;
        protected int _sleepTime = 200;
        protected uint monopolyCommanId;

        public event EventHandler<ServerResponseReceivedEventArgs> ServerResponseReceived;

        public InteractDispatcher(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection", "cannot be null");
            }
            this._receiveInProgress = false;
            this._continueRecieve = false;
            this._filter = new Dictionary<string, List<uint>>();
            this._nextId = 0;
            this._connection = connection;
            this._queues = new Dictionary<uint, Queue<string>>();
            this._commandFilters = new Dictionary<uint, ResponseFilter>();
            this._mode = EDispatcherMode.Regular;
        }

        protected void AddFilter(string response, uint commandId)
        {
            lock (this)
            {
                if (!this._filter.ContainsKey(response))
                {
                    this._filter[response] = new List<uint>();
                }
                this._filter[response].Add(commandId);
            }
        }

        public virtual void Close()
        {
            lock (this)
            {
                this._connection.Close();
            }
        }

        protected void DeleteCommand(uint uid)
        {
            lock (this)
            {
                if (this._commandFilters.ContainsKey(uid))
                {
                    foreach (string str in this._commandFilters[uid])
                    {
                        if (this._filter.ContainsKey(str))
                        {
                            this._filter[str].Remove(uid);
                            if (this._filter[str].Count == 0)
                            {
                                this._filter.Remove(str);
                            }
                        }
                    }
                }
                this._commandFilters.Remove(uid);
                this._queues.Remove(uid);
            }
        }

        public void GetAccess()
        {
            this._mode = EDispatcherMode.Regular;
        }

        public void GetMonopolyAccess()
        {
            this._mode = EDispatcherMode.Monopoly;
        }

        public virtual byte[] GetRawData()
        {
            return this._connection.ReceiveBytes();
        }

        public virtual byte[] GetRawData(ulong size)
        {
            return this._connection.ReceiveBytes(size);
        }

        public virtual IMAP4Response GetResponse(uint commandId)
        {
            IMAP4Response response;
            bool flag = false;
            do
            {
                if (this._queues[commandId].Count == 0)
                {
                    lock (this)
                    {
                        if (!this._receiveInProgress)
                        {
                            this.GetServerData();
                        }
                    }
                    if (this._receiveInProgress)
                    {
                        Task.Run(async() => await Task.Delay(this._sleepTime)).Wait();
                    }
                }
                else
                {
                    flag = true;
                }
            }
            while (!flag);
            lock (this)
            {
                response = new IMAP4Response(this._queues[commandId].Dequeue().Replace("\r\n", ""));
            }
            if (response.IsCompletionResponse())
            {
                this.DeleteCommand(commandId);
            }
            return response;
        }

        protected void GetServerData()
        {
            this._receiveInProgress = true;
            try
            {
                string str;
                lock (this._connection)
                {
                    str = this._connection.ReceiveLine();
                }
                lock (this)
                {
                    IMAP4Response response = new IMAP4Response(str);
                    if (response.IsCompletionResponse())
                    {
                        uint num = uint.Parse(response.Tag);
                        this._queues[num].Enqueue(str);
                    }
                    else if (response.Type == EIMAP4ResponseType.Continuation)
                    {
                        this._queues[this.monopolyCommanId].Enqueue(str);
                    }
                    else
                    {
                        string name = response.Name;
                        if (this._filter.ContainsKey(name))
                        {
                            List<uint> list = this._filter[name];
                            foreach (uint num2 in list)
                            {
                                this._queues[num2].Enqueue(str);
                            }
                        }
                        else
                        {
                            this.OnServerResponseReceived(response);
                        }
                    }
                }
            }
            finally
            {
                this._receiveInProgress = false;
            }
        }

        protected void OnServerResponseReceived(IMAP4Response response)
        {
            EventHandler<ServerResponseReceivedEventArgs> serverResponseReceived = this.ServerResponseReceived;
            if (serverResponseReceived != null)
            {
                serverResponseReceived(this, new ServerResponseReceivedEventArgs(response));
            }
        }

        public virtual void Open()
        {
            this._connection.Open();
        }

        public void ReleaseAccess()
        {
            this._mode = EDispatcherMode.Regular;
        }

        public void ReleaseMonopolyAccess()
        {
            this._mode = EDispatcherMode.Regular;
        }

        public virtual uint SendCommand(string command)
        {
            return this.SendCommand(command, new ResponseFilter());
        }

        public virtual uint SendCommand(string command, ResponseFilter filter)
        {
            uint commandId = this._nextId++;
            foreach (string str in filter)
            {
                this.AddFilter(str, commandId);
            }
            if (!this._commandFilters.ContainsKey(commandId))
            {
                this._commandFilters.Add(commandId, filter);
            }
            lock (this)
            {
                this._connection.SendLine(string.Format("{0} {1}", commandId, command));
            }
            this._queues[commandId] = new Queue<string>();
            this.monopolyCommanId = ((this._mode & EDispatcherMode.Monopoly) == EDispatcherMode.Monopoly) ? commandId : uint.MaxValue;
            return commandId;
        }

        public virtual uint SendContinuationCommand(string command)
        {
            uint num = this._nextId++;
            lock (this)
            {
                this._connection.SendLine(command);
            }
            this._queues[num] = new Queue<string>();
            return num;
        }

        public virtual void SwitchToSslChannel()
        {
            if (this._receiveInProgress)
            {
                throw new InvalidOperationException("Can't switch to ssl while receive in progress");
            }
            lock (this)
            {
                this._connection.SwitchToSslChannel();
            }
        }

        public IConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        [Flags]
        protected enum EDispatcherMode
        {
            Monopoly = 4,
            Regular = 8
        }
    }
}

