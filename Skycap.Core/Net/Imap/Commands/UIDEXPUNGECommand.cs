namespace Skycap.Net.Imap.Commands
{
    using System;
    using Skycap.Net.Imap.Responses;
    using Skycap.Net.Imap.Sequences;

    internal class UIDEXPUNGECommand : IMAP4BaseCommand
    {
        protected readonly SequenceSet sequence;

        public UIDEXPUNGECommand(ISequence sequence)
        {
            this.sequence = new SequenceSet();
            this.sequence.Add(sequence);
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Concat(new object[] { "UID EXPUNGE ", this.sequence }));
            return new CompletionResponse(base._dispatcher.GetResponse(commandId).Response);
        }
    }
}
