namespace Skycap.Net.Pop3
{
    using System;

    public class Pop3Response
    {
        protected string _message;
        protected string _response;
        protected EPop3ResponseType _type;

        public Pop3Response()
        {
            this.Type = EPop3ResponseType.OK;
        }

        public Pop3Response(string message)
        {
            if (message == null)
            {
                this.Type = EPop3ResponseType.ERR;
                this.Response = "The server doesn't return any response";
            }
            else
            {
                this.Response = message;
            }
        }

        public Pop3Response(EPop3ResponseType type, string errorMessage)
        {
            this._message = (errorMessage == null) ? "" : errorMessage;
            this._response = this._message;
            this.Type = type;
        }

        protected virtual string ParseResponse(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (message.StartsWith("+OK"))
            {
                this.Type = EPop3ResponseType.OK;
                return message.Remove(0, 3).Trim();
            }
            this.Type = EPop3ResponseType.ERR;
            if (message.StartsWith("-ERR"))
            {
                return message.Remove(0, 4).Trim();
            }
            return message;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this._response))
            {
                return this._response;
            }
            return this.Type.ToString();
        }

        public virtual string Message
        {
            get
            {
                return this._message;
            }
        }

        protected string Response
        {
            get
            {
                return this._response;
            }
            set
            {
                this._response = value;
                this._message = this.ParseResponse(value);
            }
        }

        public virtual EPop3ResponseType Type
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

