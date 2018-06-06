namespace Skycap.Net.Imap.Commands
{
    using System;
    using System.Text;

    internal abstract class BaseAUTHENTICATECommand : IMAP4BaseCommand
    {
        protected string _password;
        protected string _username;
        protected const string UnexpectedResponseMessage = "Unexpected response";

        protected BaseAUTHENTICATECommand()
        {
        }

        protected override void EndCommand()
        {
            base._dispatcher.ReleaseMonopolyAccess();
        }

        protected string GetBase64String(string source)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }

        protected override void StartCommand()
        {
            base._dispatcher.GetMonopolyAccess();
        }
    }
}

