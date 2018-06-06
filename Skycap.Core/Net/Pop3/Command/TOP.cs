namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using Skycap.Net.Pop3.Exceptions;
    using System;

    public class TOP : Pop3Command
    {
        protected string _messageBody;
        protected string _messageHeaders;
        protected const string exIncorectServerResponse = "Incorrect server response";
        protected uint messageNumber;
        protected uint messageRowCount;

        public TOP(uint number, uint rows)
        {
            this.messageNumber = number;
            this.messageRowCount = rows;
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            string message = connection.ReceiveLine();
            if (message == ".")
            {
                message = connection.ReceiveLine();
            }
            Pop3Response response = new Pop3Response(message);
            if (response.Type == EPop3ResponseType.OK)
            {
                message = "";
                while ((message.Length == 0) || !message.EndsWith("\r\n\r\n"))
                {
                    string str2 = connection.ReceiveLine();
                    message = message + str2 + "\r\n";
                }
                this.MessageHeaders = message.Remove(message.Length - ("\r\n".Length * 2), "\r\n".Length * 2);
                if (this.messageRowCount != 0)
                {
                    message = "";
                }
                while ((message.Length == 0) || !message.EndsWith("\r\n\r\n\r\n.\r\n"))
                {
                    string str3 = connection.ReceiveLine();
                    message = message + str3 + "\r\n";
                }
                message = message.Remove((message.Length - ("\r\n".Length * 4)) - 1, ("\r\n".Length * 4) + 1);
                if (this.messageRowCount != 0)
                {
                    this.MessageBody = message;
                }
                return response;
            }
            if ((response.Type == EPop3ResponseType.ERR) && (string.Compare("no such message", response.Message, StringComparison.CurrentCultureIgnoreCase) != 0))
            {
                throw new Pop3ReceiveException("Incorrect server response");
            }
            return response;
        }

        protected override string CommandText
        {
            get
            {
                if (string.IsNullOrEmpty(base.name))
                {
                    base.name = string.Format("TOP {0} {1}", this.messageNumber, this.messageRowCount);
                }
                return base.name;
            }
        }

        public virtual string MessageBody
        {
            get
            {
                return this._messageBody;
            }
            protected set
            {
                this._messageBody = value;
            }
        }

        public virtual string MessageHeaders
        {
            get
            {
                return this._messageHeaders;
            }
            protected set
            {
                this._messageHeaders = value;
            }
        }
    }
}

