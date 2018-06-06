namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;

    public class MAILCommand : ISmtpAction
    {
        protected readonly EmailAddress _addressFrom;
        protected const string CommandPattern = "MAIL FROM:{0}";

        public MAILCommand(EmailAddress addressFrom)
        {
            this._addressFrom = addressFrom;
        }

        public virtual SmtpResponse Interact(IConnection connection)
        {
            connection.SendLine(string.Format("MAIL FROM:{0}", this._addressFrom.GetEmailString()));
            return new SmtpResponse(connection);
        }
    }
}

