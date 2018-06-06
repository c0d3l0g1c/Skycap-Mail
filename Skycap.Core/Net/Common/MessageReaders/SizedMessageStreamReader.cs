namespace Skycap.Net.Common.MessageReaders
{
    using Skycap.Net.Common.Exceptions;
    using Skycap.Net.Common.Extensions;
    using System;
    using System.IO;

    public class SizedMessageStreamReader : IMessageReader
    {
        private bool _endOfMessage;
        private readonly Stream _incomingStream;
        private uint _readedSize;
        private readonly uint _size;

        public SizedMessageStreamReader(Stream incomingStream, uint size)
        {
            if (incomingStream == null)
            {
                throw new ArgumentNullException("incomingStream");
            }
            this._incomingStream = incomingStream;
            this._size = size;
            this._readedSize = 0;
            this._endOfMessage = false;
        }

        public byte[] ReadLine()
        {
            if (this.EndOfMessage)
            {
                throw new EndOfMessageException();
            }
            if (this._size <= this._readedSize)
            {
                this._endOfMessage = true;
                return null;
            }
            uint maxBytesToRead = this._size - this._readedSize;
            byte[] buffer = StreamExtensions.ReadToEndLine(this._incomingStream, maxBytesToRead);
            if (buffer == null)
            {
                this._endOfMessage = true;
                return null;
            }
            this._readedSize += (uint) (buffer.Length + 2);
            return buffer;
        }

        public bool EndOfMessage
        {
            get
            {
                return this._endOfMessage;
            }
        }
    }
}

