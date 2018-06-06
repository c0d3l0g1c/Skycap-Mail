namespace Skycap.Net.Pop3
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Common.Exceptions;
    using Skycap.Net.Common.MessageReaders;
    using System;

    public class Pop3MessageStreamReader : IMessageReader
    {
        protected IConnection _connection;
        protected bool _endOfMessage;
        public const string _exReadAfterEndOfMessage = "End of the message was reached";

        public Pop3MessageStreamReader(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            this._connection = connection;
            this.EndOfMessage = false;
        }

        public byte[] ReadLine()
        {
            if (this.EndOfMessage)
            {
                throw new EndOfMessageException("End of the message was reached");
            }
            byte[] sourceArray = this._connection.ReceiveBytes();
            if ((sourceArray.Length == 1) && (sourceArray[0] == 0x2e))
            {
                this.EndOfMessage = true;
                return null;
            }
            if ((sourceArray.Length > 1) && (sourceArray[0] == 0x2e))
            {
                byte[] destinationArray = new byte[sourceArray.Length - 1];
                Array.Copy(sourceArray, 1, destinationArray, 0, sourceArray.Length - 1);
                return destinationArray;
            }
            return sourceArray;
        }

        public bool EndOfMessage
        {
            get
            {
                return this._endOfMessage;
            }
            protected set
            {
                this._endOfMessage = value;
            }
        }
    }
}

