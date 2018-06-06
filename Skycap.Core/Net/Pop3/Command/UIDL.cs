namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using Skycap.Net.Pop3.Exceptions;
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [DebuggerDisplay("Messages = {Messages.Count}")]
    public class UIDL : Pop3Command
    {
        protected uint _messageNumber;
        protected Pop3MessageUIDInfoCollection _messages;
        protected static Regex _regMessageInfo = new Regex("([0-9]+) (.+)");
        protected bool _selectOneMessage;
        protected const string exIncorectServerResponse = "Incorrect server response";

        public UIDL()
        {
            this.Init();
        }

        public UIDL(uint number)
        {
            this._selectOneMessage = true;
            this._messageNumber = number;
            this.Init();
        }

        protected virtual Pop3MessageUIDInfo GetOneMessage(string response)
        {
            uint num;
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            Match match = _regMessageInfo.Match(response);
            if ((!match.Success || (match.Groups.Count <= 2)) || !uint.TryParse(match.Groups[1].Value, out num))
            {
                throw new Pop3ReceiveException("Incorrect server response");
            }
            return new Pop3MessageUIDInfo(num, match.Groups[2].Value);
        }

        protected virtual void Init()
        {
            if (this._selectOneMessage)
            {
                base.name = string.Format("UIDL {0}", this._messageNumber);
            }
            else
            {
                base.name = "UIDL";
            }
            this.Messages = new Pop3MessageUIDInfoCollection();
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            string message = connection.ReceiveLine();
            Pop3Response response = new Pop3Response(message);
            if (response.Type != EPop3ResponseType.ERR)
            {
                Pop3MessageUIDInfo oneMessage;
                if (!this._selectOneMessage)
                {
                    message = connection.ReceiveLine();
                    while (message != ".")
                    {
                        oneMessage = this.GetOneMessage(message);
                        if (oneMessage != null)
                        {
                            this.Messages.Add(oneMessage);
                        }
                        message = connection.ReceiveLine();
                    }
                    return response;
                }
                oneMessage = this.GetOneMessage(message);
                if (oneMessage != null)
                {
                    this.Messages.Add(oneMessage);
                }
            }
            return new Pop3Response(message);
        }

        public virtual Pop3MessageUIDInfoCollection Messages
        {
            get
            {
                return this._messages;
            }
            protected set
            {
                this._messages = value;
            }
        }
    }
}

