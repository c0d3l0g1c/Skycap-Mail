namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Responses;
    using Skycap.Net.Imap.Sequences;
    using System;

    internal class UIDCOPYCommand : IMAP4BaseCommand
    {
        protected readonly string mailbox;
        protected readonly SequenceSet sequence;

        public UIDCOPYCommand(ISequence sequence, string mailbox)
        {
            this.mailbox = mailbox;
            this.sequence = new SequenceSet();
            this.sequence.Add(sequence);
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("UID COPY {0} \"{1}\"", this.sequence, this.mailbox));
            return new CompletionResponse(base._dispatcher.GetResponse(commandId).Response);
        }
    }
}

