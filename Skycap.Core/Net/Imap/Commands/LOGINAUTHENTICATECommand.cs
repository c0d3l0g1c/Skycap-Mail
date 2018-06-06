namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class LOGINAUTHENTICATECommand : BaseAUTHENTICATECommand
    {
        public LOGINAUTHENTICATECommand(string username, string password)
        {
            base._username = username;
            base._password = password;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand("AUTHENTICATE LOGIN");
            if (base._dispatcher.GetResponse(commandId).Type != EIMAP4ResponseType.Continuation)
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            base._dispatcher.SendContinuationCommand(base.GetBase64String(base._username));
            if (base._dispatcher.GetResponse(commandId).Type != EIMAP4ResponseType.Continuation)
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            base._dispatcher.SendContinuationCommand(base.GetBase64String(base._password));
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!CompletionResponse.IsCompletionResponse(response.Response))
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

