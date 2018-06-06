namespace Skycap.Net.Common.MessageReaders
{
    using Skycap.Net.Common;
    using System;
    using System.Collections.Generic;

    public class Base64MessageReader : BoundedMessageReader
    {
        private readonly List<byte> _buffer;
        private EBoundaryType _reachedBoundary;

        public Base64MessageReader(IMessageReader sourceReader, string boundary) : base(sourceReader, boundary)
        {
            this._buffer = new List<byte>();
            this._reachedBoundary = EBoundaryType.NotBoundary;
        }

        public override byte[] ReadLine()
        {
            if (base.EndOfMessage)
            {
                throw new InvalidOperationException("End of the message was reached");
            }
            int endOfLinePosition = base.GetEndOfLinePosition(this._buffer, 0);
            if (this._reachedBoundary == EBoundaryType.NotBoundary)
            {
                while (endOfLinePosition == -1)
                {
                    byte[] line = base._sourceReader.ReadLine();
                    if (base._sourceReader.EndOfMessage)
                    {
                        base._endOfMessage = true;
                        base._finalBoundaryReached = true;
                        break;
                    }
                    EBoundaryType type = BoundaryChecker.CheckBoundary(line, base._boundary);
                    if (type != EBoundaryType.NotBoundary)
                    {
                        this._reachedBoundary = type;
                        byte[] buffer2 = this._buffer.ToArray();
                        this._buffer.Clear();
                        return buffer2;
                    }
                    byte[] collection = MailMessageRFCDecoder.DecodeFromBase64(line);
                    this._buffer.AddRange(collection);
                    endOfLinePosition = base.GetEndOfLinePosition(this._buffer, (this._buffer.Count - collection.Length) - 1);
                }
            }
            else
            {
                base._endOfMessage = true;
                base._finalBoundaryReached = this._reachedBoundary == EBoundaryType.Final;
                return null;
            }
            List<byte> range = this._buffer.GetRange(0, endOfLinePosition);
            this._buffer.RemoveRange(0, endOfLinePosition + 2);
            return range.ToArray();
        }
    }
}

