namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class UNSUBSCRIBECommand : IMAP4BaseCommand
    {
        protected string _mailbox;
        protected const string SubscribePattern = "UNSUBSCRIBE \"{0}\"";
        protected const string UnexpectedResponseMessage = "Unexpected response";

        public UNSUBSCRIBECommand(string mailbox)
        {
            this._mailbox = mailbox;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("UNSUBSCRIBE \"{0}\"", this._mailbox));
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return new CompletionResponse(response.Response);
        }
    }
}

