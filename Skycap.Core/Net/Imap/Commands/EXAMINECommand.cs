namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class EXAMINECommand : BaseSelectCommand
    {
        protected ResponseFilter filter;

        public EXAMINECommand(string mailbox) : base(mailbox)
        {
            this.filter = new ResponseFilter(new string[] { "FLAGS", "EXISTS", "RECENT", "OK" });
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand("EXAMINE " + base.mailbox, this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            while (!response.IsCompletionResponse())
            {
                base.ProcessResponse(response);
                response = base._dispatcher.GetResponse(commandId);
            }
            if (!base._unseenRecieved)
            {
                throw new ExpectedResponseException("Expected * OK [UNSEEN]");
            }
            if (!base._uidnextRecieved)
            {
                throw new ExpectedResponseException("Expected * OK [UIDNEXT <n>]");
            }
            if (!base._uidvalidityRecieved)
            {
                throw new ExpectedResponseException("Expected * OK [UIDVALIDITY <n>]");
            }
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
            if (!base._permanentFlagsRecieved)
            {
                throw new ExpectedResponseException("Expected * OK [PERMANENTFLAGS (list)]");
            }
            CompletionResponse response2 = new CompletionResponse(response.Response);
            base.ProcessCompletionResponse(response2);
            if (base._access == EIMAP4MailBoxAccessType.ReadWrite)
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return response2;
        }
    }
}

