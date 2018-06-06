namespace Skycap.Net.Imap.Scripts
{
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Commands;
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Parsers;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Windows.Storage;

    internal class ReceivePart : BaseFETCHCommand
    {
        private readonly string _attachmentDirectory;
        private ImapMessage _message;
        private readonly IPart _partDescription;

        public ReceivePart(ImapMessage message, IPart partDescription, string attachmentDirectory)
        {
            this._message = message;
            this._partDescription = partDescription;
            this._attachmentDirectory = attachmentDirectory;
        }

        protected override CompletionResponse Behaviour()
        {
            MessageDescriptionParser parser = new MessageDescriptionParser();
            StringBuilder builder = new StringBuilder();
            int num = 0;
            List<string> subPartList = (this._partDescription == null ? new List<string>() { "" } : this.GetSubPartList(this._partDescription));
            for (int i = 0; i < subPartList.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(" ");
                }
                builder.Append(string.Format("BODY.PEEK[{0}]", subPartList[i]));
            }
            uint commandId = base._dispatcher.SendCommand(string.Format("UID FETCH {0} ({1})", this._message.Uid, builder), base.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                if (response.Name != "FETCH")
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                string input = response.Data.Substring(response.Data.IndexOf("(") + 1);
                while (num < subPartList.Count)
                {
                    Regex regex = new Regex(@"[\}]*{(?<size>[0-9]+)}|(?<size>NIL)");
                    if (!regex.IsMatch(input))
                    {
                        throw new Exception("Unexpected response");
                    }
                    string s = regex.Match(input).Groups["size"].Value;
                    num++;
                    if (!s.Equals("NIL", StringComparison.OrdinalIgnoreCase))
                    {
                        ulong size = ulong.Parse(s);
                        byte[] rawData = base._dispatcher.GetRawData(size);
                        byte[] bytes = Encoding.UTF8.GetBytes(input + "\r\n");
                        byte[] destinationArray = new byte[bytes.Length + rawData.Length];
                        Array.Copy(bytes, destinationArray, bytes.Length);
                        Array.Copy(rawData, 0, destinationArray, bytes.Length, rawData.Length);
                        this._message = parser.Parse(this._message, destinationArray, this._message.Uid, this._attachmentDirectory);
                        byte[] rawDataBytes = base._dispatcher.GetRawData();
                        input = Encoding.UTF8.GetString(rawDataBytes, 0, rawDataBytes.Length);
                    }
                }
                response = base._dispatcher.GetResponse(commandId);
            }
            return new CompletionResponse(response.Response);
        }

        private List<string> GetSubPartList(IPart rootPart)
        {
            List<string> list = new List<string>();
            switch (rootPart.Type)
            {
                case EPartType.Multi:
                {
                    MultiPart part = rootPart as MultiPart;
                    if (part != null)
                    {
                        foreach (IPart part2 in part.Parts)
                        {
                            list.AddRange(this.GetSubPartList(part2));
                        }
                    }
                    return list;
                }
                case EPartType.Content:
                    return list;
            }
            list.Add(this._message.GetPartIndex(rootPart));
            return list;
        }

        public IPart Part
        {
            get
            {
                return this._partDescription;
            }
        }
    }
}

