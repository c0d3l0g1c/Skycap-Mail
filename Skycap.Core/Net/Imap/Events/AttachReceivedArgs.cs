namespace Skycap.Net.Imap.Events
{
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Imap;
    using System;

    public class AttachReceivedArgs : EventArgs
    {
        protected Attachment _attachmentDescription;
        protected ImapMessage _message;

        public AttachReceivedArgs(ImapMessage message, Attachment attachmentDescription)
        {
            this._message = message;
            this._attachmentDescription = attachmentDescription;
        }

        public ImapMessage Message
        {
            get
            {
                return this._message;
            }
        }

        public Attachment ReceivedAttachmentDescription
        {
            get
            {
                return this._attachmentDescription;
            }
        }
    }
}

