namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;
    using System.Text;

    public class AUTHPlainCommand : ISmtpAction
    {
        private readonly string _login;
        private readonly string _password;

        public AUTHPlainCommand(string login, string password)
        {
            this._login = login;
            this._password = password;
        }

        public virtual SmtpResponse Interact(IConnection connection)
        {
            string s = "\0" + this._login + "\0" + this._password;
            connection.SendLine("AUTH PLAIN " + Convert.ToBase64String(Encoding.UTF8.GetBytes(s)));
            return new AuthCommandResponse(connection, EAuthCommandStage.CredentialsSended);
        }
    }
}

