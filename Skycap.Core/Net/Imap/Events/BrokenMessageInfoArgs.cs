namespace Skycap.Net.Imap.Events
{
    using System;

    public class BrokenMessageInfoArgs : EventArgs
    {
        protected string _message;
        protected long _size;
        protected uint _uid;

        public BrokenMessageInfoArgs(string message)
        {
            this._message = message;
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

