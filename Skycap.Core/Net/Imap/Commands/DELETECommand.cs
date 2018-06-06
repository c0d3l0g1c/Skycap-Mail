namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class DELETECommand : IMAP4BaseCommand
    {
        protected string _mailbox;
        protected const string DeletePattern = "DELETE \"{0}\"";
        protected const string UnexpectedResponseMessage = "Unexpected response";

        public DELETECommand(string mailbox)
        {
            this._mailbox = mailbox;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format(DeletePattern, this._mailbox));
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return new CompletionResponse(response.Response);
        }
    }
}

