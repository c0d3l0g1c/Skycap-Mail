namespace Skycap.Net.Pop3
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("Number = {Number}, Size = {Size}")]
    public class Pop3MessageInfo
    {
        protected uint _number;
        protected uint _size;

        public Pop3MessageInfo(uint number, uint size)
        {
            this.Number = number;
            this.Size = size;
        }

        public override string ToString()
        {
            return (this.Number + " " + this.Size);
        }

        public virtual uint Number
        {
            get
            {
                return this._number;
            }
            protected set
            {
                this._number = value;
            }
        }

        public virtual uint Size
        {
            get
            {
                return this._size;
            }
            protected set
            {
                this._size = value;
            }
        }
    }
}

