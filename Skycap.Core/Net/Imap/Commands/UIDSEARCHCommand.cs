namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using Skycap.Net.Imap.Sequences;
    using System;
    using System.Text;

    internal class UIDSEARCHCommand : IMAP4BaseCommand
    {
        protected MessageSequenceNumbers _uids;
        protected Encoding encoding;
        protected ResponseFilter filter = new ResponseFilter(new string[] { "SEARCH" });
        protected Query query;
        protected const string UnexpectedResponseMessage = "Unexpected response";

        public UIDSEARCHCommand(Encoding encoding, Query query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query", "cannot be null");
            }
            this.encoding = encoding;
            this.query = query;
            this._uids = new MessageSequenceNumbers();
        }

        protected override CompletionResponse Behaviour()
        {
            string str;
            if (this.encoding == null)
            {
                str = this.query.ToString();
            }
            else
            {
                str = string.Concat(new object[] { "CHARSET ", this.encoding.WebName, " ", this.query });
            }
            uint commandId = base._dispatcher.SendCommand("UID SEARCH " + str, this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            this._uids = new MessageSequenceNumbers();
            if (!response.IsCompletionResponse())
            {
                if (response.Name != "SEARCH")
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                this._uids = this.ParseResponse(response);
                response = base._dispatcher.GetResponse(commandId);
                if (!response.IsCompletionResponse())
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
            }
            return new CompletionResponse(response.Response);
        }

        private MessageSequenceNumbers ParseResponse(IMAP4Response response)
        {
            MessageSequenceNumbers numbers = new MessageSequenceNumbers();
            string data = response.Data;
            if (data.IndexOf(' ') != -1)
            {
                foreach (string str2 in data.Substring(data.IndexOf(' ') + 1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    numbers.Add(new SequenceNumber(uint.Parse(str2)));
                }
            }
            return numbers;
        }

        public MessageSequenceNumbers Uids
        {
            get
            {
                return this._uids;
            }
        }
    }
}

