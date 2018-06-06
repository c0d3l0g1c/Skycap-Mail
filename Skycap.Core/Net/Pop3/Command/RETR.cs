namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Common.Parsers;
    using Skycap.Net.Pop3;
    using Skycap.Net.Pop3.Exceptions;
    using System;
    using System.Diagnostics;
    using System.Text;
    using Windows.Storage;

    [DebuggerDisplay("Number = {messageNumber}")]
    public class RETR : Pop3Command
    {
        protected string _attachmentDirectory;
        protected PopMessage _message;
        protected const string exIncorectServerResponse = "Incorrect server response";
        protected uint messageNumber;
        protected string uid;

        public RETR(uint number, string uid, string attachmentDirectory)
        {
            this.messageNumber = number;
            this.uid = uid;
            this.AttachmentDirectory = attachmentDirectory;
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            byte[] bytes = connection.ReceiveBytes();
            Pop3Response response = new Pop3Response(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            if (response.Type == EPop3ResponseType.OK)
            {
                Pop3MessageStreamReader reader = new Pop3MessageStreamReader(connection);
                this.Message = new MailMessageParser().Parse(reader, uid, this.AttachmentDirectory);
                return response;
            }
            if ((response.Type == EPop3ResponseType.ERR) && (string.Compare("no such message", response.Message, StringComparison.CurrentCultureIgnoreCase) != 0))
            {
                throw new Pop3ReceiveException("Incorrect server response");
            }
            return response;
        }

        public virtual string AttachmentDirectory
        {
            get
            {
                return this._attachmentDirectory;
            }
            protected set
            {
                this._attachmentDirectory = value;
            }
        }

        protected override string CommandText
        {
            get
            {
                if (string.IsNullOrEmpty(base.name))
                {
                    base.name = string.Format("RETR {0}", this.messageNumber);
                }
                return base.name;
            }
        }

        public virtual PopMessage Message
        {
            get
            {
                return this._message;
            }
            protected set
            {
                this._message = value;
            }
        }
    }
}

