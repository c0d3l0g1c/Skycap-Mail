namespace Skycap.Net.Pop3.Exceptions
{
    using Skycap.Net.Common;
    using System;

    public class Pop3ReceiveMessageException : Exception
    {
        private PopMessage _mailMessage;

        public Pop3ReceiveMessageException(string errorMessage, PopMessage mailMessage) : base(errorMessage)
        {
            this.MailMessage = mailMessage;
        }

        public PopMessage MailMessage
        {
            get
            {
                return this._mailMessage;
            }
            protected set
            {
                this._mailMessage = value;
            }
        }
    }
}

