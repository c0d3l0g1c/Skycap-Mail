namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Collections.Generic;

    internal class CAPABIILITYCommand : IMAP4BaseCommand
    {
        protected List<string> capabilities;
        protected ResponseFilter filter = new ResponseFilter(new string[] { "CAPABILITY" });
        protected const string UnexpectedResponseMessage = "Unexpected response";

        protected override CompletionResponse Behaviour()
        {
            CompletionResponse response2;
            this.capabilities = new List<string>();
            uint commandId = base._dispatcher.SendCommand("CAPABILITY", this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if ((response.Name == "CAPABILITY") && (response.Type == EIMAP4ResponseType.Untagged))
            {
                if (response.Data.IndexOf(' ') != -1)
                {
                    string[] collection = response.Data.Substring(response.Name.Length + 1).Split(new char[] { ' ' });
                    this.capabilities = new List<string>(collection);
                }
                response = base._dispatcher.GetResponse(commandId);
                if (!CompletionResponse.IsCompletionResponse(response.Response))
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                response2 = new CompletionResponse(response.Response);
                if (response2.CompletionResult == ECompletionResponseType.NO)
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                return response2;
            }
            if (!CompletionResponse.IsCompletionResponse(response.Response))
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            response2 = new CompletionResponse(response.Response);
            if (response2.CompletionResult != ECompletionResponseType.BAD)
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return response2;
        }

        public List<string> Capabilities
        {
            get
            {
                return this.capabilities;
            }
        }
    }
}

