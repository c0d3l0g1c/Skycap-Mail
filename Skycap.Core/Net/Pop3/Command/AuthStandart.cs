namespace Skycap.Net.Pop3.Command
{
    using Skycap.Net.Common.Connections;
    using Skycap.Net.Pop3;
    using System;

    public class AuthStandart : Pop3Command
    {
        protected PASS _pass;
        protected USER _user;

        public AuthStandart(string username, string password)
        {
            this._user = new USER(username);
            this._pass = new PASS(password);
        }

        public override Pop3Response Interact(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            Pop3Response response = this._user.Interact(connection);
            if (response.Type == EPop3ResponseType.OK)
            {
                response = this._pass.Interact(connection);
            }
            return response;
        }
    }
}

