namespace Skycap.Net.Imap.Responses
{
    using System;
    using System.Text.RegularExpressions;

    public class IMAP4Response
    {
        protected string _data;
        protected string _name;
        protected string _response;
        protected string _tag;
        private EIMAP4ResponseType _type;

        public IMAP4Response(string message)
        {
            if (!IsFormatCorrect(message))
            {
                throw new FormatException("IMAP4 Server response does not match RFC");
            }
            this._response = message;
            string[] strArray = message.Split(new char[] { ' ' });
            this._data = "";
            if (message.IndexOf(' ') != -1)
            {
                this._data = message.Substring(message.IndexOf(' ') + 1);
            }
            this._name = "";
            this._tag = strArray[0];
            if (strArray[0] == "+")
            {
                this.Type = EIMAP4ResponseType.Continuation;
            }
            else if (strArray[0] == "*")
            {
                this.Type = EIMAP4ResponseType.Untagged;
            }
            else
            {
                this.Type = EIMAP4ResponseType.Tagged;
            }
            if (this.Type != EIMAP4ResponseType.Continuation)
            {
                int num;
                this._name = int.TryParse(strArray[1], out num) ? strArray[2] : strArray[1];
            }
        }

        public bool IsCompletionResponse()
        {
            return CompletionResponse.IsCompletionResponse(this.Response);
        }

        public static bool IsFormatCorrect(string responseString)
        {
            new Regex(@"(\+.*)|((\*|[a-zA-Z0-9]+)\s(([0-9]+\s.+)|([^0-9].*)))");
            return Regex.Match(responseString, @"(\+.*)|((\*|[a-zA-Z0-9]+)\s(([0-9]+\s.+)|([^0-9].*)))", RegexOptions.IgnoreCase).Success;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this._response))
            {
                return this._response;
            }
            return this.Type.ToString();
        }

        public string Data
        {
            get
            {
                return this._data;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string Response
        {
            get
            {
                return this._response;
            }
        }

        public string Tag
        {
            get
            {
                return this._tag;
            }
        }

        public EIMAP4ResponseType Type
        {
            get
            {
                return this._type;
            }
            protected set
            {
                this._type = value;
            }
        }
    }
}

