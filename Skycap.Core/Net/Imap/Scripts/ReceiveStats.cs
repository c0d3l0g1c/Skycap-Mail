using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Skycap.Net.Common;
using Skycap.Net.Imap.Commands;
using Skycap.Net.Imap.Events;
using Skycap.Net.Imap.Exceptions;
using Skycap.Net.Imap.Responses;

namespace Skycap.Net.Imap.Scripts
{
    internal class ReceiveStats : BaseFETCHCommand
    {
        public event EventHandler<BrokenMessageInfoArgs> BrokenMessage;

        protected Mailbox _mailbox;
        protected IList<StatisticInfo> _statistics;
        protected static readonly Regex serialNumberRegex = new Regex(@"\* (\d+) FETCH");
        protected static readonly Regex uniqueNumberRegex = new Regex(@"UID (\d+)");
        protected static readonly Regex messageSizeRegex = new Regex(@"RFC822.SIZE (\d+)");
        protected static readonly Regex flagsRegex = new Regex(@"FLAGS \((.*)\).");

        public ReceiveStats(Mailbox mailbox)
        {
            this._mailbox = mailbox;
            this._statistics = new List<StatisticInfo>();
        }

        public IEnumerable<StatisticInfo> Statistics
        {
            get
            {
                return _statistics.AsEnumerable();
            }
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(@"UID FETCH 1:* (FLAGS UID RFC822.SIZE)", base.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                if (response.Name != "FETCH")
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                while (!response.IsCompletionResponse())
                {
                    try
                    {
                        this._statistics.Add(ParseResponse(response));
                    }
                    catch (Exception exception)
                    {
                        if (this.BrokenMessage != null)
                            this.BrokenMessage(this, new BrokenMessageInfoArgs(exception.Message));
                    }
                    response = base._dispatcher.GetResponse(commandId);
                }
            }
            return new CompletionResponse(response.Response);
        }

        private StatisticInfo ParseResponse(IMAP4Response response)
        {
            uint serialNumber = uint.Parse(serialNumberRegex.Match(response.Response).Groups[1].Value);
            string uniqueNumber = uniqueNumberRegex.Match(response.Response).Groups[1].Value;
            uint messageSize = uint.Parse(messageSizeRegex.Match(response.Response).Groups[1].Value);
            string flags = flagsRegex.Match(response.Response).Groups[1].Value;
            return new StatisticInfo(uniqueNumber, serialNumber, messageSize, flags);
        }
    }
}
