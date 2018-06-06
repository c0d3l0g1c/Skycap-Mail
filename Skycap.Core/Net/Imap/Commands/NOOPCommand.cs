namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Responses;
    using System;

    internal class NOOPCommand : IMAP4BaseCommand
    {
        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand("NOOP");
            return new CompletionResponse(base._dispatcher.GetResponse(commandId).Response);
        }
    }
}

