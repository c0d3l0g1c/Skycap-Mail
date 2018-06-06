namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using Skycap.Net.Pop3.Exceptions;
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    [DebuggerDisplay("Name = {name}")]
    public abstract class Pop3Command : IPOP3Command
    {
        protected string name;
        public static readonly Regex regDigest = new Regex(@"<[^\s]+>");
        public static readonly Regex regUInt = new Regex("[0-9]+");

        protected Pop3Command()
        {
        }

        public virtual Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            Pop3Response response = new Pop3Response(connection.ReceiveLine());
            if (response.Type == EPop3ResponseType.ERR)
            {
                throw new Pop3ReceiveException(string.Format("Request error {0}:{1}", this.CommandText, response.Message));
            }
            return response;
        }

        protected virtual string CommandText
        {
            get
            {
                if (string.IsNullOrEmpty(this.name))
                {
                    throw new Pop3Exception("Command name is not specified");
                }
                return this.name;
            }
        }
    }
}

