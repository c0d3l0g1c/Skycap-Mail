namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;

    public class QUITCommand : ISmtpAction
    {
        protected const string CommandPattern = "QUIT";

        public virtual SmtpResponse Interact(IConnection connection)
        {
            connection.SendLine("QUIT");
            return new SmtpResponse(connection);
        }
    }
}

