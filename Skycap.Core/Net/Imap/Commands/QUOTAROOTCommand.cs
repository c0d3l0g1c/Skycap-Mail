using System;
using System.Text.RegularExpressions;
using Skycap.Net.Common;
using Skycap.Net.Imap.Exceptions;
using Skycap.Net.Imap.Responses;

namespace Skycap.Net.Imap.Commands
{
    internal class QUOTAROOTCommand : IMAP4BaseCommand
    {
        protected Mailbox _mailbox;
        protected ResponseFilter filter = new ResponseFilter(new string[] { "QUOTA" });
        private static Regex quotaRegex = new Regex(@"\* QUOTA .* \(STORAGE (\d+) (\d+)\)");

        public QUOTAROOTCommand(Mailbox mailbox)
        {
            this._mailbox = mailbox;
        }

        public uint Size
        {
            get;
            private set;
        }

        public uint LimitSize
        {
            get;
            private set;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("GETQUOTAROOT \"{0}\"", this._mailbox.FullName), this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                if (response.Name != "QUOTA")
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                this.ParseResponse(response);
                response = base._dispatcher.GetResponse(commandId);
                if (!response.IsCompletionResponse())
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
            }
            return new CompletionResponse(response.Response);
        }

        private void ParseResponse(IMAP4Response response)
        {
            Match match = quotaRegex.Match(response.Response);
            Size = uint.Parse(match.Groups[1].Value);
            LimitSize = uint.Parse(match.Groups[2].Value);
        }
    }
}
