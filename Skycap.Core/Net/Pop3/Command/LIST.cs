namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using Skycap.Net.Pop3.Exceptions;
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [DebuggerDisplay("Messages = {Messages.Count}")]
    public class LIST : Pop3Command
    {
        protected Pop3MessageInfoCollection _messages;
        protected const string exIncorectServerResponse = "Incorrect server response";
        protected uint messageNumber;
        protected bool selectOneMessage;

        public LIST()
        {
            this.Init();
        }

        public LIST(uint number)
        {
            this.selectOneMessage = true;
            this.messageNumber = number;
            this.Init();
        }

        protected virtual Pop3MessageInfo GetOneMessage(string response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            MatchCollection matchs = Pop3Command.regUInt.Matches(response);
            uint result = 0;
            uint num2 = 0;
            new Pop3Response(response);
            if (((matchs.Count == 2) && uint.TryParse(matchs[0].Value, out num2)) && uint.TryParse(matchs[1].Value, out result))
            {
                return new Pop3MessageInfo(num2, result);
            }
            return null;
        }

        protected virtual void Init()
        {
            if (this.selectOneMessage)
            {
                base.name = string.Format("LIST {0}", this.messageNumber);
            }
            else
            {
                base.name = "LIST";
            }
            this.Messages = new Pop3MessageInfoCollection();
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            STAT stat = new STAT();
            stat.Interact(connection);
            connection.SendLine(this.CommandText);
            string str = connection.ReceiveLine();
            if (this.selectOneMessage)
            {
                Pop3MessageInfo oneMessage = this.GetOneMessage(str);
                if (oneMessage != null)
                {
                    this.Messages.Add(oneMessage);
                }
            }
            else
            {
                Pop3Response response = new Pop3Response(str);
                if (response.Type == EPop3ResponseType.OK)
                {
                    MatchCollection matchs = Pop3Command.regUInt.Matches(response.Message);
                    if (stat.MessagesCount > 0)
                    {
                        for (int i = 0; i < stat.MessagesCount; i++)
                        {
                            str = connection.ReceiveLine();
                            Pop3MessageInfo item = this.GetOneMessage(str);
                            if (item != null)
                            {
                                this.Messages.Add(item);
                            }
                        }
                        connection.ReceiveLine();
                        return response;
                    }
                    return response;
                }
            }
            return new Pop3Response(str);
        }

        public virtual Pop3MessageInfoCollection Messages
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

