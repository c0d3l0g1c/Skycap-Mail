namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class LOGOUTCommand : IMAP4BaseCommand
    {
        protected ResponseFilter filter = new ResponseFilter(new string[] { "BYE" });
        protected const string UnexpectedResponseMessage = "Unexpected response";

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand("LOGOUT", this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (CompletionResponse.IsCompletionResponse(response.Response))
            {
                CompletionResponse response2 = new CompletionResponse(response.Response);
                if (response2.CompletionResult != ECompletionResponseType.BAD)
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                return response2;
            }
            if ((response.Name != "BYE") || (response.Type != EIMAP4ResponseType.Untagged))
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            response = base._dispatcher.GetResponse(commandId);
            if (!CompletionResponse.IsCompletionResponse(response.Response))
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            CompletionResponse response3 = new CompletionResponse(response.Response);
            if (response3.CompletionResult == ECompletionResponseType.NO)
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return response3;
        }
    }
}

