namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;

    public class StartTlsCommand : ISmtpAction
    {
        public virtual SmtpResponse Interact(IConnection connection)
        {
            connection.SendLine("STARTTLS");
            return new SmtpResponse(connection);
        }
    }
}

