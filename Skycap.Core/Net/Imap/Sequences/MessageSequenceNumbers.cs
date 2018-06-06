namespace Skycap.Net.Imap.Sequences
{
    using System;
    using System.Collections.Generic;

    public class MessageSequenceNumbers : List<SequenceNumber>, ISequence
    {
        public MessageSequenceNumbers()
        {
        }

        public MessageSequenceNumbers(IEnumerable<uint> sourceNumbers)
        {
            foreach (uint num in sourceNumbers)
            {
                base.Add(new SequenceNumber(num));
            }
        }

        public override string ToString()
        {
            bool flag = true;
            string str = "";
            foreach (SequenceNumber number in this)
            {
                if (!flag)
                {
                    str = str + ",";
                }
                else
                {
                    flag = false;
                }
                str = str + number;
            }
            return string.IsNullOrEmpty(str) ? "1:*" : str;
        }
    }
}

