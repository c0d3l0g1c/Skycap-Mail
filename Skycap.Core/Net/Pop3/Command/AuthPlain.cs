namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;
    using System.Text;

    public class AuthPlain : Pop3Command
    {
        protected string _login;
        protected string _password;

        public AuthPlain(string login, string password)
        {
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            this._login = login;
            this._password = password;
        }

        public override Pop3Response Interact(IConnection connection)
        {
            connection.SendLine(this.CommandText);
            connection.ReceiveLine();
            string s = "\0" + this._login + "\0" + this._password;
            connection.SendLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(s)));
            return new Pop3Response(connection.ReceiveLine());
        }

        protected override string CommandText
        {
            get
            {
                return "AUTH PLAIN";
            }
        }
    }
}

