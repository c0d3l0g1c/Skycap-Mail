namespace Skycap.Net.Imap.Sequences
{
    using System;

    public class SequenceRange : ISequence
    {
        protected ISequence left;
        protected ISequence right;

        public SequenceRange(ISequence left, ISequence right)
        {
            this.left = left;
            this.right = right;
        }

        public override string ToString()
        {
            return (this.left.ToString() + ":" + this.right.ToString());
        }
    }
}

