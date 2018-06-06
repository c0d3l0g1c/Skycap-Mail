namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("Messages = {messageNumber}")]
    public class DELE : Pop3Command
    {
        protected uint messageNumber;

        public DELE(uint message_id)
        {
            this.messageNumber = message_id;
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            string message = connection.ReceiveLine();
            if (message == ".")
            {
                message = connection.ReceiveLine();
            }
            return new Pop3Response(message);
        }

        protected override string CommandText
        {
            get
            {
                if (string.IsNullOrEmpty(base.name))
                {
                    base.name = string.Format("DELE {0}", this.messageNumber);
                }
                return base.name;
            }
        }
    }
}

