namespace Skycap.Net.Common.MessageReaders
{
    using Skycap.Net.Common;
    using System;
    using System.Collections.Generic;

    public class QuotedPrintableMessageReader : BoundedMessageReader
    {
        private readonly List<byte> _buffer;

        public QuotedPrintableMessageReader(IMessageReader sourceReader, string boundary) : base(sourceReader, boundary)
        {
            this._buffer = new List<byte>();
        }

        public override byte[] ReadLine()
        {
            byte[] collection = new byte[] { 13, 10 };
            if (base.EndOfMessage)
            {
                throw new InvalidOperationException("End of the message was reached");
            }
            int endOfLinePosition = base.GetEndOfLinePosition(this._buffer, 0);
            while (endOfLinePosition == -1)
            {
                bool flag;
                byte[] buffer3;
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
                    base._endOfMessage = true;
                    base._finalBoundaryReached = type == EBoundaryType.Final;
                    return this._buffer.ToArray();
                }
                if ((line.Length > 0) && (line[line.Length - 1] == 0x3d))
                {
                    flag = false;
                    buffer3 = new byte[line.Length - 1];
                    Array.Copy(line, 0, buffer3, 0, line.Length - 1);
                }
                else
                {
                    flag = true;
                    buffer3 = line;
                }
                byte[] buffer4 = MailMessageRFCDecoder.DecodeFromQuotedPrintable(buffer3);
                this._buffer.AddRange(buffer4);
                if (flag)
                {
                    this._buffer.AddRange(collection);
                }
                int offset = (this._buffer.Count - buffer4.Length) - 1;
                offset -= flag ? 2 : 0;
                endOfLinePosition = base.GetEndOfLinePosition(this._buffer, offset);
            }
            List<byte> range = this._buffer.GetRange(0, endOfLinePosition);
            this._buffer.RemoveRange(0, endOfLinePosition + 2);
            return range.ToArray();
        }
    }
}

