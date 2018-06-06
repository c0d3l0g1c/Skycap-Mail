namespace Skycap.Net.Imap.Sequences
{
    using System;

    public class SequenceNumber : ISequence
    {
        protected uint number;

        public SequenceNumber(uint number)
        {
            if (number == 0)
            {
                throw new ArgumentOutOfRangeException("number", "number must be positive");
            }
            this.number = number;
        }

        public override string ToString()
        {
            return this.number.ToString();
        }

        public uint ToUint()
        {
            return this.number;
        }
    }
}

