namespace Skycap.Net.Smtp.Exceptions
{
    using Skycap.Net.Smtp;
    using System;

    public class UnexpectedSMTPResponseException : Exception
    {
        protected SmtpResponse _response;

        public UnexpectedSMTPResponseException(string message) : base(message)
        {
        }

        public UnexpectedSMTPResponseException(string message, SmtpResponse response) : base(message)
        {
            this.Response = response;
        }

        public UnexpectedSMTPResponseException(string message, SmtpResponse response, Exception inner) : base(message, inner)
        {
            this.Response = response;
        }

        public virtual SmtpResponse Response
        {
            get
            {
                return this._response;
            }
            set
            {
                this._response = value;
            }
        }
    }
}

