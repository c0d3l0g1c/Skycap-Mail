namespace Skycap.Net.Common.MessageReaders
{
    using Skycap.Net.Common;
    using System;

    public class UnencodedMessageReader : BoundedMessageReader
    {
        public UnencodedMessageReader(IMessageReader reader, string boundary) : base(reader, boundary)
        {
            base._sourceReader = reader;
            base._boundary = boundary;
            base._finalBoundaryReached = false;
        }

        public override byte[] ReadLine()
        {
            if (base.EndOfMessage)
            {
                throw new InvalidOperationException("End of the message was reached");
            }
            byte[] line = base._sourceReader.ReadLine();
            if (base._sourceReader.EndOfMessage)
            {
                base._endOfMessage = true;
                base._finalBoundaryReached = true;
                return null;
            }
            EBoundaryType type = BoundaryChecker.CheckBoundary(line, base._boundary);
            if (type != EBoundaryType.NotBoundary)
            {
                base._finalBoundaryReached = type == EBoundaryType.Final;
                base._endOfMessage = true;
                return null;
            }
            return line;
        }
    }
}

