namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;

    public class HELOCommand : ISmtpAction
    {
        public virtual SmtpResponse Interact(IConnection connection)
        {
            string textLine = "HELO " + EHLOCommand.GetFQDN();
            connection.SendLine(textLine);
            return new SmtpResponse(connection);
        }
    }
}

