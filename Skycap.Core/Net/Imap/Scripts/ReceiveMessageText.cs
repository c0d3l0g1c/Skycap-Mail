namespace Skycap.Net.Imap.Scripts
{
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Sequences;
    using System;

    internal class ReceiveMessageText
    {
        private readonly string _attchmentDirectory;
        protected ImapMessage _message;
        protected SequenceNumber _uid;

        public ReceiveMessageText(SequenceNumber uid, string attchmentDirectory)
        {
            this._uid = uid;
            this._attchmentDirectory = attchmentDirectory;
        }

        public void Run(IInteractDispatcher dispatcher)
        {
            ReceiveHeader header = new ReceiveHeader(this._uid, true, this._attchmentDirectory);
            header.Interact(dispatcher);
            this._message = header.MessageCollection[0];
            new ReceivePart(this._message, this._message.RootPart, this._attchmentDirectory).Interact(dispatcher);
        }

        public ImapMessage Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

