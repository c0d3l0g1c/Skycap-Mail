namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class CHECKCommand : IMAP4BaseCommand
    {
        protected const string UnexpectedResponseMessage = "Unexpected response";

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand("CHECK");
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            CompletionResponse response2 = new CompletionResponse(response.Response);
            if (response2.CompletionResult == ECompletionResponseType.NO)
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return response2;
        }
    }
}

