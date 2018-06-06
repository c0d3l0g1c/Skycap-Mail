namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Collections;
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal abstract class BaseListCommand : IMAP4BaseCommand
    {
        protected readonly string cmd;
        protected ResponseFilter filter;
        protected readonly string mailboxName;
        protected MatchedNameCollection matches;
        protected readonly string referenceName;
        protected const string UnexpectedResponseMessage = "Unexpected response";

        public BaseListCommand(string cmd, string referenceName, string mailboxName)
        {
            this.cmd = cmd;
            this.referenceName = referenceName;
            this.mailboxName = mailboxName;
        }

        protected override CompletionResponse Behaviour()
        {
            this.matches = new MatchedNameCollection();
            uint commandId = base._dispatcher.SendCommand(string.Format("{0} \"{1}\" \"{2}\"", this.cmd, this.referenceName, this.mailboxName), this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            while (!response.IsCompletionResponse())
            {
                if ((response.Name != this.cmd) || (response.Type != EIMAP4ResponseType.Untagged))
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                this.matches.Add(new MatchedName(response));
                response = base._dispatcher.GetResponse(commandId);
            }
            return new CompletionResponse(response.Response);
        }

        public MatchedNameCollection MatchedNames
        {
            get
            {
                return this.matches;
            }
        }
    }
}

