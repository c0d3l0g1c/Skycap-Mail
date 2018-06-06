namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;

    public class RCPTCommand : ISmtpAction
    {
        protected readonly EmailAddress _addressTo;
        protected const string CommandPattern = "RCPT TO:{0}";

        public RCPTCommand(EmailAddress addressTo)
        {
            this._addressTo = addressTo;
        }

        public virtual SmtpResponse Interact(IConnection connection)
        {
            connection.SendLine(string.Format("RCPT TO:{0}", this._addressTo.GetEmailString()));
            return new SmtpResponse(connection);
        }
    }
}

