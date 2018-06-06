namespace Skycap.Net.Imap.Parsers
{
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Common.MessageReaders;
    using Skycap.Net.Common.Parsers;
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Collections;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Windows.Storage;

    internal class MessageDescriptionParser
    {
        private const string _exIncorrectFormat = "The source string was not in a correct format";
        private readonly Encoding DefaultEncoding = new UTF8Encoding();

        public void AddItem(ImapMessage messageDescription, string itemKey, byte[] itemValue, string attachmentDirectory, string uid)
        {
            string str;
            itemKey = itemKey.ToLower();
            switch (itemKey)
            {
                case "flags":
                    if (messageDescription.Header.Date == DateTime.MinValue
                     && string.IsNullOrEmpty(messageDescription.Header.MessageID)
                     && messageDescription.Header.From == null
                     && messageDescription.Header.To.Count == 0)
                    {
                        IEnumerable<MessageFlag> collection = this.ParseMessageFlagCollection(Net.Imap.Parsers.Utils.Unparenthesis(itemValue));
                        messageDescription.Flags.AddRange(collection);
                    }
                    return;

                case "rfc822.size":
                    str = this.DefaultEncoding.GetString(itemValue, 0, itemValue.Length);
                    messageDescription.Size = uint.Parse(str);
                    return;

                case "body":
                    messageDescription.RootPart = new BodyPartDescriptionParser().Parse(Net.Imap.Parsers.Utils.Unparenthesis(itemValue), attachmentDirectory);
                    return;

                case "body[header]":
                    messageDescription.Header = new MIMEHeaderParser().Parse(this.DefaultEncoding.GetString(itemValue, 0, itemValue.Length));
                    return;
            }
            messageDescription.Uid = uid;
            Match match = new Regex(@"body\[(?<index>[\d\.]+)\]").Match(itemKey);
            if (match.Success)
            {
                IPart part = messageDescription.FindPart(match.Groups["index"].Value);
                if (part.Type == EPartType.Text)
                {
                    TextPart part2 = (TextPart) part;
                    string charset = part2.Header.ContentType.Attributes.ContainsKey("charset") ? part2.Header.ContentType.Attributes["charset"] : "us-ascii";
                    part2.Text = new SimplePartContentParser().Parse(itemValue, part2.Header.ContentTransferEncoding, charset);
                }
                else if (part.Type == EPartType.Message)
                {
                    MessagePart part3 = (MessagePart) part;
                    MemoryStream incomingStream = new MemoryStream(itemValue);
                    SizedMessageStreamReader reader = new SizedMessageStreamReader(incomingStream, (uint) itemValue.Length);
                    part3.Message = new MailMessageParser().Parse(reader, uid, attachmentDirectory);
                }
            }
            match = new Regex(@"body\[(?<index>[0-9\.]+)\.mime\]").Match(itemKey);
            if (match.Success)
            {
                messageDescription.FindPart(match.Groups["index"].Value).Header = new MIMEHeaderParser().Parse(this.DefaultEncoding.GetString(itemValue, 0, itemValue.Length));
            }
        }

        public ImapMessage Parse(byte[] source, string uid, string attachmentDirectory)
        {
            ImapMessage sourceMessage = new ImapMessage();
            this.Parse(sourceMessage, source, uid, attachmentDirectory);
            if (sourceMessage.RootPart != null)
            {
                sourceMessage.InitializeInternalStructure();
            }
            return sourceMessage;
        }

        public ImapMessage Parse(ImapMessage sourceMessage, byte[] source, string uid, string attachmentDirectory)
        {
            List<byte[]> list = Net.Imap.Parsers.Utils.ExtractParams(source);
            if ((list.Count % 2) != 0)
            {
                throw new FormatException("The source string was not in a correct format");
            }
            for (int i = 0; i < list.Count; i += 2)
            {
                this.AddItem(sourceMessage, this.DefaultEncoding.GetString(list[i], 0, list[i].Length), list[i + 1], attachmentDirectory, uid);
            }
            sourceMessage.FillText();
            return sourceMessage;
        }

        public MessageFlagCollection ParseMessageFlagCollection(byte[] source)
        {
            MessageFlagCollection flags = new MessageFlagCollection();
            Regex regex = new Regex(@"\\([^\s]+)");
            string input = this.DefaultEncoding.GetString(source, 0, source.Length);
            foreach (Match match in regex.Matches(input))
            {
                flags.Add(new MessageFlag(match.Groups[1].Value));
            }
            return flags;
        }
    }
}

