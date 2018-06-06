namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap;
    using System;

    public class MessageReceivedEventArgs : EventArgs
    {
        protected ImapMessage _message;

        public MessageReceivedEventArgs(ImapMessage message)
        {
            this._message = message;
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

