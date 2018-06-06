namespace Skycap.Net.Imap.Commands
{
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Collections.Generic;

    internal class STATUSCommand : IMAP4BaseCommand
    {
        protected readonly string command;
        protected ResponseFilter filter = new ResponseFilter(new string[] { "STATUS" });
        protected int messages;
        protected int recent;
        protected long uidnext;
        protected long uidvalidity;
        protected const string UnexpectedResponseMessage = "Unexpected response";
        protected int unseen;

        public STATUSCommand(string mailbox, IEnumerable<EIMAP4StatusRequest> requests)
        {
            List<string> list = new List<string>();
            using (IEnumerator<EIMAP4StatusRequest> enumerator = requests.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    switch (enumerator.Current)
                    {
                        case EIMAP4StatusRequest.Messages:
                        {
                            list.Add("MESSAGES");
                            continue;
                        }
                        case EIMAP4StatusRequest.Recent:
                        {
                            list.Add("RECENT");
                            continue;
                        }
                        case EIMAP4StatusRequest.UidNext:
                        {
                            list.Add("UIDNEXT");
                            continue;
                        }
                        case EIMAP4StatusRequest.UidValidity:
                        {
                            list.Add("UIDVALIDITY");
                            continue;
                        }
                    }
                    list.Add("UNSEEN");
                }
            }
            this.command = string.Format("STATUS \"{0}\" ({1})", mailbox, string.Join(" ", list.ToArray()));
            this.messages = -1;
            this.recent = -1;
            this.unseen = -1;
            this.uidnext = -1L;
            this.uidvalidity = -1L;
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(this.command, this.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if ((response.Name == "STATUS") && (response.Type == EIMAP4ResponseType.Untagged))
            {
                int index = response.Data.IndexOf('(');
                int num3 = response.Data.IndexOf(')');
                if ((index != -1) && (num3 != -1))
                {
                    string[] strArray = response.Data.Substring(index + 1, (num3 - index) - 1).Split(new char[] { ' ' });
                    if ((strArray.Length % 2) != 0)
                    {
                        throw new UnexpectedResponseException("Unexpected response");
                    }
                    for (int i = 0; i < strArray.Length; i += 2)
                    {
                        string str2 = strArray[i];
                        if (str2 == "MESSAGES")
                        {
                            this.messages = int.Parse(strArray[i + 1]);
                        }
                        else if (str2 == "RECENT")
                        {
                            this.recent = int.Parse(strArray[i + 1]);
                        }
                        else if (str2 == "UIDNEXT")
                        {
                            this.uidnext = long.Parse(strArray[i + 1]);
                        }
                        else if (str2 == "UIDVALIDITY")
                        {
                            this.uidvalidity = long.Parse(strArray[i + 1]);
                        }
                        else
                        {
                            if (str2 != "UNSEEN")
                            {
                                throw new UnexpectedResponseException("Unexpected response");
                            }
                            this.unseen = int.Parse(strArray[i + 1]);
                        }
                    }
                }
            }
            else
            {
                if (!CompletionResponse.IsCompletionResponse(response.Response))
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                return new CompletionResponse(response.Response);
            }
            response = base._dispatcher.GetResponse(commandId);
            if (!CompletionResponse.IsCompletionResponse(response.Response))
            {
                throw new UnexpectedResponseException("Unexpected response");
            }
            return new CompletionResponse(response.Response);
        }

        public int Messages
        {
            get
            {
                return this.messages;
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
                return this.uidnext;
            }
        }

        public long UidValidity
        {
            get
            {
                return this.uidvalidity;
            }
        }

        public int Unseen
        {
            get
            {
                return this.unseen;
            }
        }
    }
}

