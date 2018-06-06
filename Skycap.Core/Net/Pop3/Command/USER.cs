namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;

    public class USER : Pop3Command
    {
        protected string login;

        public USER(string login)
        {
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            this.login = login;
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
                    base.name = string.Format("USER {0}", this.login);
                }
                return base.name;
            }
        }
    }
}

