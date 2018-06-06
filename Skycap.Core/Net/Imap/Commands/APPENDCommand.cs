using System;
using System.Collections.Generic;
using Skycap.Net.Imap.Exceptions;
using Skycap.Net.Imap.Responses;

namespace Skycap.Net.Imap.Commands
{
    internal class APPENDCommand : IMAP4BaseCommand
    {
        private readonly string mailbox;
        private readonly string message;

        public APPENDCommand(string mailbox, string message)
        {
            this.mailbox = mailbox;
            this.message = message;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("APPEND \"{0}\" {1}{2}{3}", this.mailbox, "{" + message.Length + "}", Environment.NewLine, message));
            IMAP4Response response = null;
            try
            {
                response = base._dispatcher.GetResponse(commandId);
            }
            catch
            {
                base._dispatcher.SendCommand(this.message);
                response = base._dispatcher.GetResponse(commandId);
            }
            if (!response.IsCompletionResponse())
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return new CompletionResponse(response.Response);
        }
    }
}
