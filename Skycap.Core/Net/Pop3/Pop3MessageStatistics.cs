namespace Skycap.Net.Pop3
{
    using System;

    public class Pop3MessageStatistics
    {
        protected uint _countMessages;
        protected uint _sizeMessages;

        public Pop3MessageStatistics()
        {
            this.CountMessages = 0;
            this.SizeMessages = 0;
        }

        public Pop3MessageStatistics(uint countMessages, uint sizeMessages)
        {
            this.CountMessages = countMessages;
            this.SizeMessages = sizeMessages;
        }

        public void Reset()
        {
            this.CountMessages = 0;
            this.SizeMessages = 0;
        }

        public virtual uint CountMessages
        {
            get
            {
                return this._countMessages;
            }
            protected set
            {
                this._countMessages = value;
            }
        }

        public virtual uint SizeMessages
        {
            get
            {
                return this._sizeMessages;
            }
            protected set
            {
                this._sizeMessages = value;
            }
        }
    }
}

