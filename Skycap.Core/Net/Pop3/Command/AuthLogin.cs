namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;
    using System.Text;

    public class AuthLogin : Pop3Command
    {
        protected string _login;
        protected string _password;

        public AuthLogin(string login, string password)
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
            string textLine = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._login));
            connection.SendLine(textLine);
            connection.ReceiveLine();
            string str2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._password));
            connection.SendLine(str2);
            return new Pop3Response(connection.ReceiveLine());
        }

        protected override string CommandText
        {
            get
            {
                return "AUTH LOGIN";
            }
        }
    }
}

