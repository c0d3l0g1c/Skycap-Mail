namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class LOGINCommand : BaseAUTHENTICATECommand
    {
        protected const string LoginPattern = "LOGIN \"{0}\" \"{1}\"";

        public LOGINCommand(string username, string password)
        {
            base._username = username;
            base._password = password;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("LOGIN \"{0}\" \"{1}\"", base._username, base._password));
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return new CompletionResponse(response.Response);
        }
    }
}

