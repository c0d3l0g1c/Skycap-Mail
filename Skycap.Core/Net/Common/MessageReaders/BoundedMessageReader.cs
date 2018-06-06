namespace Skycap.Net.Common.MessageReaders
{
    using System;
    using System.Collections.Generic;

    public abstract class BoundedMessageReader : IMessageReader
    {
        protected string _boundary;
        protected bool _endOfMessage = false;
        public const string _exReadAfterEndOfMessage = "End of the message was reached";
        protected bool _finalBoundaryReached;
        protected IMessageReader _sourceReader;

        protected BoundedMessageReader(IMessageReader sourceReader, string boundary)
        {
            this._sourceReader = sourceReader;
            this._boundary = boundary;
        }

        protected int GetEndOfLinePosition(IList<byte> source, int offset)
        {
            offset = (offset > 0) ? offset : 0;
            for (int i = offset + 1; i < source.Count; i++)
            {
                if ((source[i - 1] == 13) && (source[i] == 10))
                {
                    return (i - 1);
                }
            }
            return -1;
        }

        public abstract byte[] ReadLine();

        public bool EndOfMessage
        {
            get
            {
                return this._endOfMessage;
            }
        }

        public bool FinalBoundaryReached
        {
            get
            {
                return this._finalBoundaryReached;
            }
        }
    }
}

