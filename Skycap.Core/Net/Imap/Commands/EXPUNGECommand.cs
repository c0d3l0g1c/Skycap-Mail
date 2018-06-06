namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Collections.Generic;

    internal class EXPUNGECommand : IMAP4BaseCommand
    {
        protected ResponseFilter filter = new ResponseFilter(new string[] { "EXPUNGE" });
        protected List<int> removed = new List<int>();
        protected const string UnexpectedResponseMessage = "Unexpected response";

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand("EXPUNGE", this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            while (!response.IsCompletionResponse())
            {
                if (response.Name != "EXPUNGE")
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                int item = int.Parse(response.Data.Split(new char[] { ' ' })[0]);
                this.removed.Add(item);
                response = base._dispatcher.GetResponse(commandId);
            }
            return new CompletionResponse(response.Response);
        }

        public List<int> Removed
        {
            get
            {
                return this.removed;
            }
        }
    }
}

