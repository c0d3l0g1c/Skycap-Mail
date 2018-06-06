namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Text.RegularExpressions;

    internal abstract class BaseFETCHCommand : IMAP4BaseCommand
    {
        protected ResponseFilter filter = new ResponseFilter(new string[] { "FETCH" });
        protected const string sizeParserPattern = @"\{(?<size>[0-9]+)\}";
        protected const string UnexpectedResponseMessage = "Unexpected response";

        protected BaseFETCHCommand()
        {
        }

        protected override void EndCommand()
        {
            base._dispatcher.ReleaseMonopolyAccess();
        }

        protected ulong GetSize(IMAP4Response response)
        {
            Regex regex = new Regex(@"\{(?<size>[0-9]+)\}");
            if (!regex.IsMatch(response.Data))
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return ulong.Parse(regex.Match(response.Data).Groups["size"].Value);
        }

        protected override void StartCommand()
        {
            base._dispatcher.GetMonopolyAccess();
        }
    }
}

