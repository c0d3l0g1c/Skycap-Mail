namespace Skycap.Net.Imap.Scripts
{
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Sequences;
    using System;
    using Windows.Storage;
    using System.IO;
    using Skycap.Net.Common;

    internal class ReceiveFullMessage
    {
        protected string _attachmentDirectory;
        protected ImapMessage _message;
        protected SequenceNumber _uid;

        public ReceiveFullMessage(SequenceNumber uid, string attachmentDirectory)
        {
            this._uid = uid;
            this._attachmentDirectory = Path.Combine(attachmentDirectory, MailClient.MessageFolderPrefix + uid.ToString());
        }

        public void Run(IInteractDispatcher dispatcher)
        {
            ReceiveMessageText text = new ReceiveMessageText(this._uid, this._attachmentDirectory);
            text.Run(dispatcher);
            this._message = text.Message;
            foreach (Attachment attachment in this._message.Attachments)
            {
                new ReceiveAttach(this._message, attachment, this._attachmentDirectory).Interact(dispatcher);
            }
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

