namespace Skycap.Net.Imap.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Common.Parsers;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Windows.Storage;

    internal class BodyPartDescriptionParser
    {
        private const string _exIncorrectFormat = "The source string was not in a correct format";
        private static readonly UTF8Encoding DefaultEncoding = new UTF8Encoding();

        public string DecodeParameterValue(string source)
        {
            bool flag = false;
            bool flag2 = true;
            StringBuilder builder = new StringBuilder();
            foreach (string str in source.Replace("\\\"", "").Split(new char[] { ' ' }))
            {
                bool flag3 = flag;
                string str2 = MailMessageRFCDecoder.ParseBase64Item(str);
                if (str2 != "")
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                    str2 = str;
                }
                if ((!flag || !flag3) && !flag2)
                {
                    builder.Append(' ');
                }
                flag2 = false;
                builder.Append(str2);
            }
            return builder.ToString();
        }

        protected virtual bool IsMessagePart(ContentType contentType)
        {
            return contentType.Type.Equals("message", StringComparison.OrdinalIgnoreCase);
        }

        protected virtual bool IsTextPart(ContentType contentType)
        {
            return (contentType.Type.Equals("text", StringComparison.OrdinalIgnoreCase) && !contentType.Attributes.ContainsKey("filename"));
        }

        public bool IsValidMultipart(byte[] source)
        {
            Regex regex = new Regex("\\A\\([\\s\\S]*\\) \"[^\"]+\"\\Z");
            return regex.IsMatch(Encoding.UTF8.GetString(source, 0, source.Length));
        }

        public bool IsValidSimplePart(byte[] source)
        {
            return true;
        }

        public IPart Parse(byte[] source, string attachmentDirectory)
        {
            IPart part = null;
            if (this.IsValidMultipart(source))
            {
                return this.ParseMultipart(source, attachmentDirectory);
            }
            if (this.IsValidSimplePart(source))
            {
                part = this.ParseSimplePart(source, attachmentDirectory);
            }
            return part;
        }

        public Dictionary<string, string> ParseAttributes(byte[] source)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (!DefaultEncoding.GetString(source, 0, source.Length).Equals("NIL", StringComparison.OrdinalIgnoreCase))
            {
                List<byte[]> list = Skycap.Net.Imap.Parsers.Utils.ExtractParams(source);
                if ((list.Count % 2) != 0)
                {
                    throw new FormatException("The source string was not in a correct format");
                }
                for (int i = 0; i < list.Count; i += 2)
                {
                    dictionary.Add(Encoding.UTF8.GetString(list[i], 0, list[i].Length).ToLower(), this.DecodeParameterValue(Encoding.UTF8.GetString(list[i + 1], 0, list[i + 1].Length)));
                }
            }
            return dictionary;
        }

        public IPart ParseContentPart(byte[] source, ContentType contentType, uint size, string attachmentDirectory)
        {
            ContentPart part = new ContentPart {
                AttachmentDirectory = attachmentDirectory,
                Size = size
            };
            if (contentType.Attributes.ContainsKey("filename"))
            {
                part.DiskFilename = contentType.Attributes["filename"];
            }
            return part;
        }

        public MultiPart ParseMultipart(byte[] source, string attachmentDirectory)
        {
            MultiPart part = new MultiPart();
            List<byte[]> list = Skycap.Net.Imap.Parsers.Utils.ExtractParams(source);
            for (int i = 0; i < (list.Count - 1); i++)
            {
                part.Parts.Add(this.Parse(list[i], attachmentDirectory));
            }
            Match match = new Regex("\\([\\s\\S]*\\) \"([^\"]+)\"").Match(Encoding.UTF8.GetString(source, 0, source.Length));
            if (!match.Success)
            {
                throw new FormatException("The source string was not in a correct format");
            }
            part.Header.ContentType = new ContentType("multipart", match.Groups[1].Value.ToLower());
            return part;
        }

        private static string ParseOptionalField(string value)
        {
            if (!value.Equals("NIL", StringComparison.OrdinalIgnoreCase))
            {
                return value.Replace("\"", "");
            }
            return null;
        }

        public IPart ParseSimplePart(byte[] source, string attachmentDirectory)
        {
            IPart part;
            List<byte[]> list = Skycap.Net.Imap.Parsers.Utils.ExtractParams(source);
            if (list.Count < 7)
            {
                throw new FormatException("The source string was not in a correct format");
            }
            ContentType contentType = new ContentType(Encoding.UTF8.GetString(list[0], 0, list[0].Length).ToLower(), Encoding.UTF8.GetString(list[1], 0, list[1].Length).ToLower());
            Dictionary<string, string> dictionary = this.ParseAttributes(list[2]);
            if (dictionary.ContainsKey("name"))
            {
                contentType.Attributes.Add("filename", dictionary["name"]);
            }
            if (dictionary.ContainsKey("charset"))
            {
                contentType.Attributes.Add("charset", dictionary["charset"]);
            }
            if (this.IsTextPart(contentType))
            {
                TextPart part2 = new TextPart();
                part = part2;
            }
            else if (this.IsMessagePart(contentType))
            {
                MessagePart part3 = new MessagePart();
                part = part3;
            }
            else
            {
                part = this.ParseContentPart(source, contentType, uint.Parse(Encoding.UTF8.GetString(list[6], 0, list[6].Length)), attachmentDirectory);
            }
            part.Header.ContentType = contentType;
            part.Header.ContentID = ParseOptionalField(Encoding.UTF8.GetString(list[3], 0, list[3].Length));
            part.Header.ContentDescription = ParseOptionalField(this.DecodeParameterValue(Encoding.UTF8.GetString(list[4], 0, list[4].Length)));
            part.Header.ContentTransferEncoding = MIMEHeaderParser.GetTransferEncodingFromString(Encoding.UTF8.GetString(list[5], 0, list[5].Length).ToLower());
            return part;
        }
    }
}

