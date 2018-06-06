namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;

    internal class RENAMECommand : IMAP4BaseCommand
    {
        private readonly string _newname;
        private readonly string _oldname;
        protected const string LoginPattern = "RENAME \"{0}\" \"{1}\"";
        protected const string UnexpectedResponseMessage = "Unexpected response";

        public RENAMECommand(string oldName, string newName)
        {
            this._oldname = oldName;
            this._newname = newName;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("RENAME \"{0}\" \"{1}\"", this._oldname, this._newname));
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return new CompletionResponse(response.Response);
        }
    }
}

