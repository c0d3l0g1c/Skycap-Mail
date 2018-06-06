namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;

    public class PASS : Pop3Command
    {
        protected string password;

        public PASS(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            this.password = password;
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            connection.SendLine(this.CommandText);
            return new Pop3Response(connection.ReceiveLine());
        }

        protected override string CommandText
        {
            get
            {
                if (string.IsNullOrEmpty(base.name))
                {
                    base.name = string.Format("PASS {0}", this.password);
                }
                return base.name;
            }
        }
    }
}

