namespace Skycap.Net.Smtp.ServerActions
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Smtp;
    using System;
    using System.Text;

    public class AUTHLoginCommand : ISmtpAction
    {
        private readonly string _login;
        private readonly string _password;

        public AUTHLoginCommand(string login, string password)
        {
            this._login = login;
            this._password = password;
        }

        public virtual SmtpResponse Interact(IConnection connection)
        {
            connection.SendLine("AUTH LOGIN");
            AuthCommandResponse response = new AuthCommandResponse(connection, EAuthCommandStage.AUTHSended);
            if (response.Type != ESmtpResponseType.PositiveIntermediate)
            {
                return response;
            }
            string textLine = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._login));
            connection.SendLine(textLine);
            AuthCommandResponse response2 = new AuthCommandResponse(connection, EAuthCommandStage.LoginSended);
            if (response2.Type != ESmtpResponseType.PositiveIntermediate)
            {
                return response2;
            }
            string str2 = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._password));
            connection.SendLine(str2);
            return new AuthCommandResponse(connection, EAuthCommandStage.CredentialsSended);
        }
    }
}

