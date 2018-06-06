namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using Skycap.Net.Pop3.Exceptions;
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [DebuggerDisplay("Count = {MessagesCount}, Size = {MessagesSize}")]
    public class STAT : Pop3Command
    {
        protected Pop3MessageStatistics _statistics;

        public STAT()
        {
            base.name = "STAT";
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            Pop3Response response = new Pop3Response(connection.ReceiveLine());
            this.Statistics = new Pop3MessageStatistics();
            if (response.Type == EPop3ResponseType.ERR)
            {
                throw new Pop3ReceiveException(string.Format("Request error {0}:{1}", this.CommandText, response.Message));
            }
            uint result = 0;
            uint num2 = 0;
            MatchCollection matchs = Pop3Command.regUInt.Matches(response.Message);
            if (matchs.Count == 2)
            {
                uint.TryParse(matchs[0].Value, out result);
                uint.TryParse(matchs[1].Value, out num2);
            }
            this.Statistics = new Pop3MessageStatistics(result, num2);
            return response;
        }

        public virtual uint MessagesCount
        {
            get
            {
                return this.Statistics.CountMessages;
            }
        }

        public virtual uint MessagesSize
        {
            get
            {
                return this.Statistics.SizeMessages;
            }
        }

        public virtual Pop3MessageStatistics Statistics
        {
            get
            {
                return this._statistics;
            }
            protected set
            {
                this._statistics = value;
            }
        }
    }
}

