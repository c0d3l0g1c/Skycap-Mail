namespace Skycap.Net.Imap.Sequences
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class SequenceSet : ISequence, IEnumerable<ISequence>, IEnumerable
    {
        protected List<ISequence> sequences;

        public SequenceSet()
        {
            this.Init();
        }

        public SequenceSet(IEnumerable<ISequence> sequence)
        {
            this.Init();
            foreach (ISequence sequence2 in sequence)
            {
                this.Add(sequence2);
            }
        }

        public virtual void Add(ISequence subsequence)
        {
            if (subsequence == null)
            {
                throw new ArgumentNullException("subsequence");
            }
            this.sequences.Add(subsequence);
        }

        protected void Init()
        {
            this.sequences = new List<ISequence>();
        }

        IEnumerator<ISequence> IEnumerable<ISequence>.GetEnumerator()
        {
            return this.sequences.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.sequences.GetEnumerator();
        }

        public override string ToString()
        {
            string str = "";
            bool flag = true;
            foreach (ISequence sequence in this.sequences)
            {
                if (!flag)
                {
                    str = str + "," + sequence.ToString();
                }
                else
                {
                    flag = false;
                    str = sequence.ToString();
                }
            }
            return str;
        }

        public virtual int Count
        {
            get
            {
                return this.sequences.Count;
            }
        }

        public virtual ISequence this[int index]
        {
            get
            {
                return this.sequences[index];
            }
        }
    }
}

