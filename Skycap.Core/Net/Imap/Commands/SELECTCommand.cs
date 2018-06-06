namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class SELECTCommand : BaseSelectCommand
    {
        protected ResponseFilter filter;

        public SELECTCommand(string mailbox) : base(mailbox)
        {
            this.filter = new ResponseFilter(new string[] { "FLAGS", "EXISTS", "RECENT", "OK" });
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("SELECT \"{0}\"", base.mailbox), this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            while (!response.IsCompletionResponse())
            {
                base.ProcessResponse(response);
                response = base._dispatcher.GetResponse(commandId);
            }
            CompletionResponse response2 = new CompletionResponse(response.Response);
            if (response2.CompletionResult == ECompletionResponseType.OK)
            {
                if (!base._existsRecieved)
                {
                    throw new ExpectedResponseException("Expected * <n> EXISTS");
                }
                if (!base._recentRecieved)
                {
                    throw new ExpectedResponseException("Expected * <n> RECENT");
                }
                if (!base._flagsRecieved)
                {
                    throw new ExpectedResponseException("Expected * FLAGS (list)");
                }
            }
            base.ProcessCompletionResponse(response2);
            return response2;
        }
    }
}

