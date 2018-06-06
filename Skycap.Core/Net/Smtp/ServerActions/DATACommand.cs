namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;
    using System.Collections.Generic;

    public class DATACommand : ISmtpAction
    {
        protected readonly IEnumerable<string> _messageLines;
        protected const string CommandPattern = "DATA";

        public DATACommand(IEnumerable<string> messageLines)
        {
            this._messageLines = messageLines;
        }

        public virtual SmtpResponse Interact(IConnection connection)
        {
            connection.SendLine("DATA");
            SmtpResponse response = new SmtpResponse(connection);
            if (response.Type != ESmtpResponseType.PositiveIntermediate)
            {
                return response;
            }
            foreach (string str in this._messageLines)
            {
                connection.SendLine(str);
            }
            connection.SendLine(".");
            return new SmtpResponse(connection);
        }
    }
}

