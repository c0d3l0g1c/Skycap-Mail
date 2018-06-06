namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Collections;
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Text.RegularExpressions;

    internal abstract class BaseSelectCommand : IMAP4BaseCommand
    {
        protected EIMAP4MailBoxAccessType _access;
        protected bool _accessRecieved;
        protected int _exists;
        protected bool _existsRecieved;
        protected MessageFlagCollection _flags;
        protected bool _flagsRecieved;
        protected MessageFlagCollection _permanentFlags;
        protected bool _permanentFlagsRecieved;
        protected bool _recentRecieved;
        protected long _uidnext;
        protected bool _uidnextRecieved;
        protected long _uidvalidity;
        protected bool _uidvalidityRecieved;
        protected int _unseen;
        protected bool _unseenRecieved;
        protected const string ExpectedResponseExists = "Expected * <n> EXISTS";
        protected const string ExpectedResponseFlags = "Expected * FLAGS (list)";
        protected const string ExpectedResponsePermanentFlags = "Expected * OK [PERMANENTFLAGS (list)]";
        protected const string ExpectedResponseRecent = "Expected * <n> RECENT";
        protected const string ExpectedResponseUidnext = "Expected * OK [UIDNEXT <n>]";
        protected const string ExpectedResponseUidvalidity = "Expected * OK [UIDVALIDITY <n>]";
        protected const string ExpectedResponseUnseen = "Expected * OK [UNSEEN]";
        protected static readonly string[] IgnoredResponseCode = new string[] { "HIGHESTMODSEQ", "MYRIGHTS" };
        protected readonly string mailbox;
        protected int recent;
        protected const string UnexpectedResponseMessage = "Unexpected response";

        public BaseSelectCommand(string mailbox)
        {
            this.mailbox = mailbox;
            this._unseenRecieved = false;
            this._uidnextRecieved = false;
            this._uidvalidityRecieved = false;
            this._existsRecieved = false;
            this._recentRecieved = false;
            this._flagsRecieved = false;
            this._permanentFlagsRecieved = false;
            this._accessRecieved = false;
        }

        protected override CompletionResponse Behaviour()
        {
            throw new Exception("Should be overrided");
        }

        private bool IsIgnored(string code)
        {
            foreach (string str in IgnoredResponseCode)
            {
                if (str == code)
                {
                    return true;
                }
            }
            return false;
        }

        protected void ProcessCompletionResponse(CompletionResponse response)
        {
            this._access = EIMAP4MailBoxAccessType.Default;
            if (((response.CompletionResult == ECompletionResponseType.OK) && (response.Message != "")) && (response.Message[0] == '['))
            {
                int index = response.Message.IndexOf(']');
                if (index != -1)
                {
                    string str = response.Message.Substring(0, index + 1);
                    if (str == "[READ-WRITE]")
                    {
                        this._access = EIMAP4MailBoxAccessType.ReadWrite;
                    }
                    else if (str == "[READ-ONLY]")
                    {
                        this._access = EIMAP4MailBoxAccessType.ReadOnly;
                    }
                    else
                    {
                        this._access = EIMAP4MailBoxAccessType.Default;
                    }
                    this._accessRecieved = true;
                }
            }
        }

        private void ProcessExists(IMAP4Response response)
        {
            this._exists = int.Parse(response.Data.Substring(0, response.Data.IndexOf(' ')));
            this._existsRecieved = true;
        }

        private void ProcessFlags(IMAP4Response response)
        {
            Regex regex = new Regex(@"\((?<flags>[^\)]*)\)");
            string str = regex.Match(response.Data).Groups["flags"].Value;
            this._flags = new MessageFlagCollection();
            this._flagsRecieved = true;
            if (str != "")
            {
                string[] strArray = str.Split(new char[] { ' ' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i][0] == '\\')
                        this._flags.Add(new MessageFlag(strArray[i].Substring(1)));
                }
            }
        }

        private void ProcessOK(IMAP4Response response)
        {
            string str = response.Data.Substring(response.Data.IndexOf(' ') + 1);
            if ((str.Length == 0) || (str[0] != '['))
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            if (str.IndexOf(']') == -1)
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            int index = str.IndexOf(' ');
            int num2 = str.IndexOf(']');
            string code = str.Substring(1, index - 1);
            string unseen = str.Substring(index + 1, num2 - (index + 1));
            switch (code)
            {
                case "UNSEEN":
                    this.ProcessOKUnseen(unseen);
                    return;

                case "UIDNEXT":
                    this.ProcessOKUidNext(unseen);
                    return;

                case "UIDVALIDITY":
                    this.ProcessOKUidValidity(unseen);
                    return;

                case "PERMANENTFLAGS":
                    this.ProcessOKPermanentFlags(unseen);
                    break;
            }
            if (code == "PERMANENTFLAGS")
            {
                this.ProcessOKPermanentFlags(unseen);
            }
            else if (!this.IsIgnored(code))
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
        }

        private void ProcessOKPermanentFlags(string flags)
        {
            Regex regex = new Regex(@"\((?<flags>[^\)]*)\)");
            string str = regex.Match(flags).Groups["flags"].Value;
            this._permanentFlags = new MessageFlagCollection();
            this._permanentFlagsRecieved = true;
            if (str != "")
            {
                string[] strArray = str.Split(new char[] { ' ' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (strArray[i][0] == '\\')
                        this._permanentFlags.Add(new MessageFlag(strArray[i].Substring(1)));
                }
            }
        }

        private void ProcessOKUidNext(string uidNext)
        {
            this._uidnext = long.Parse(uidNext);
            this._uidnextRecieved = true;
        }

        private void ProcessOKUidValidity(string uidValidity)
        {
            this._uidvalidity = long.Parse(uidValidity);
            this._uidvalidityRecieved = true;
        }

        private void ProcessOKUnseen(string unseen)
        {
            this._unseen = int.Parse(unseen);
            this._unseenRecieved = true;
        }

        private void ProcessRecent(IMAP4Response response)
        {
            this.recent = int.Parse(response.Data.Substring(0, response.Data.IndexOf(' ')));
            this._recentRecieved = true;
        }

        protected void ProcessResponse(IMAP4Response response)
        {
            if (response.Name == "EXISTS")
            {
                this.ProcessExists(response);
            }
            else if (response.Name == "RECENT")
            {
                this.ProcessRecent(response);
            }
            else if (response.Name == "OK")
            {
                this.ProcessOK(response);
            }
            else if (response.Name == "FLAGS")
            {
                this.ProcessFlags(response);
            }
        }

        public EIMAP4MailBoxAccessType Access
        {
            get
            {
                return this._access;
            }
        }

        public int Exists
        {
            get
            {
                return this._exists;
            }
        }

        public MessageFlagCollection Flags
        {
            get
            {
                return this._flags;
            }
        }

        public MessageFlagCollection PermanentFlags
        {
            get
            {
                return this._permanentFlags;
            }
        }

        public int Recent
        {
            get
            {
                return this.recent;
            }
        }

        public long UidNext
        {
            get
            {
                return this._uidnext;
            }
        }

        public long UidValidity
        {
            get
            {
                return this._uidvalidity;
            }
        }

        public int Unseen
        {
            get
            {
                return this._unseen;
            }
        }
    }
}

