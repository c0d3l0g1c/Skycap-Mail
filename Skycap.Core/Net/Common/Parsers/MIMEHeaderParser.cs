namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Collections;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public class MIMEHeaderParser
    {
        protected const string MIMEVersionExceptionMessage = "MIME version is not 1.0";
        public static readonly RegexOptions RO = (RegexOptions.IgnoreCase);
        private static readonly Regex regAddressAll = new Regex("((\"[^\"]+\" *<[^>]+>)|([^<,:]+ *<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?))", RO);
        private static readonly Regex regAddressDifDisplayName = new Regex("\"([^\"]+)\" *<([^>,]+)>", RO);
        private static readonly Regex regAddressDisplayName = new Regex("([^<,]+) *<([^>,]+)>", RO);
        private static readonly Regex regAddressEasy = new Regex(@" *((?:[\w\d\.!#$%&'*+\-/=?^_`{|}~-]+)@(?:[\w\d\.!#$%&'*+\-/=?^_`{|}~]+)(?:\.[a-zA-Z]{2,7})?) *", RO);
        private static readonly Regex regAllowedSymbols = new Regex(@"\A[\x00-\x7F]*\Z");
        private static readonly Regex regBCC = new Regex("Bcc: *((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?))(, ?((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?)))*", RO);
        private static readonly Regex regCC = new Regex("Cc: *((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?))(, ?((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?)))*", RO);
        private static readonly Regex regComments = new Regex(@"Comments: *([^\r]+)", RO);
        private static readonly Regex regContentDescription = new Regex(@"Content-Description: *([^\r;]+)", RO);
        private static readonly Regex regContentDisposition = new Regex(@"Content-Disposition: *([^\r;]+)[^\r]*", RO);
        private static readonly Regex regContentID = new Regex("Content-ID: *<([^>]+)>", RO);
        private static readonly Regex regContentTransferEncoding = new Regex(@"Content\-Transfer\-Encoding: *([^\r]+)", RO);
        private static readonly Regex regContentType = new Regex(@"Content-Type: *([^/]+)/([^\r;]+)[^\r]*", RO);
        private static readonly Regex regDate = new Regex(@"Date: *([A-Za-z]+,)? ?([0-9]+ ?[A-Za-z]+ ?[0-9]{4} ?[0-9]{2}:[0-9]{2}:[0-9]{2}) ?(((\+)|(-))[0-9]{4})", RO);
        public static readonly Regex regDifArgumentValue = new Regex("^\"([^\"]+)\"$", RO);
        private static readonly Regex regEasyFrom = new Regex(@"From: *([^\r]+)", RO);
        private static readonly Regex regFrom = new Regex(@"From: *([^<\r]+?<[^>]+>)", RO);
        private static readonly Regex regInReplyTo = new Regex("In-Reply-To: *<([^@]+@[^>]+)>", RO);
        private static readonly Regex regKeyValue = new Regex("([^ =:;]+) *= *\"?([^\";\\r]+)\"?", RO);
        private static readonly Regex regKeyword = new Regex("(([^\\r, ]+)|(\"[^\"]+\"))", RO);
        private static readonly Regex regKeywords = new Regex("Keywords: *((([^\\r, ]+)|(\"[^\"]+\"))(, *(([^\\r, ]+)|(\"[^\"]+\")))*)", RO);
        private static readonly Regex regMessageID = new Regex("Message-ID: *<([^@]+@[^>]+)>", RO);
        private static readonly Regex regMIMEVersion = new Regex(@"MIME-Version: *([0-9]+\.[0-9]+)", RO);
        private static readonly Regex regReferences = new Regex("References: *<([^@]+@[^>]+)>", RO);
        private static readonly Regex regReplyTo = new Regex("Reply-To: *((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?))(, ?((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?)))*", RO);
        private static readonly Regex regSender = new Regex("Sender: *((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?))", RO);
        private static readonly Regex regSubject = new Regex(@"Subject: *([^\r]+)", RO);
        private static readonly Regex regSubjectItem = new Regex("=\\?([^?]+)\\?([^?]+)\\?([^?]+)\\?=([\r\n\t]+)?", RO);
        private static readonly Regex regTo = new Regex("To: *((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?))(, ?((\"[^\"]+\" ?<[^>]+>)|([^<,:]+ ?<[^>]+>)|((?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~-]+)@(?:[\\w\\d\\.!#$%&'*+\\-/=?^_`{|}~]+)(?:\\.[a-zA-Z]{2,7})?)))*", RO);
        private static readonly Regex regXAtribbutes = new Regex(@"(X-[^:]+): ?([^\r]+)", RO);
        private static readonly Regex regImportance = new Regex(@"Importance: (Low|Normal|High)", RegexOptions.IgnoreCase);

        public static EContentTransferEncoding GetTransferEncodingFromString(string value)
        {
            switch (value.ToLowerInvariant())
            {
                case "7bit":
                    return EContentTransferEncoding.SevenBit;

                case "8bit":
                    return EContentTransferEncoding.EightBit;

                case "base64":
                    return EContentTransferEncoding.Base64;

                case "quoted-printable":
                    return EContentTransferEncoding.QuotedPrintable;

                case "binary":
                    return EContentTransferEncoding.Binary;
            }
            return EContentTransferEncoding.SevenBit;
        }

        private bool IsValidCharacters(string line)
        {
            return regAllowedSymbols.IsMatch(line);
        }

        public bool IsValidMimeLine(string line)
        {
            return MailMessageRFCDecoder.regArgument.IsMatch(line);
        }

        public MIMEHeader Parse(string source)
        {
            string str;
            MIMEHeader header = new MIMEHeader();
            Regex regex = new Regex("\r\n[ \\t]+");
            StringReader reader = new StringReader(regex.Replace(source, " "));
            header.ContentType = new ContentType();
            while ((str = reader.ReadLine()) != null)
            {
                Match match = MailMessageRFCDecoder.regArgument.Match(str);
                if (match.Success)
                {
                    switch (match.Groups[1].Value.ToLower())
                    {
                        case "mime-version":
                        {
                            Version version = this.ParseMimeVersion(str);
                            if ((version.Major != 1) || (version.Minor != 0))
                            {
                                throw new Exception("MIME version is not 1.0");
                            }
                            continue;
                        }
                        case "date":
                        {
                            header.Date = this.ParseDate(str).ToLocalTime();
                            continue;
                        }
                        case "from":
                        {
                            header.From = this.ParseFrom(str);
                            continue;
                        }
                        case "sender":
                        {
                            header.Sender = this.ParseSender(str);
                            continue;
                        }
                        case "reply-to":
                        {
                            header.ReplyTo = this.ParseReplyTo(str);
                            continue;
                        }
                        case "to":
                        {
                            header.To = this.ParseTo(str);
                            continue;
                        }
                        case "cc":
                        {
                            header.CarbonCopies = this.ParseCarbonCopies(str);
                            continue;
                        }
                        case "bcc":
                        {
                            header.BlindedCarbonCopies = this.ParseBlindCarbonCopies(str);
                            continue;
                        }
                        case "message-id":
                        {
                            header.MessageID = this.ParseMessageID(str);
                            continue;
                        }
                        case "in-reply-to":
                        {
                            header.InReplyTo = this.ParseInReplyTo(str);
                            continue;
                        }
                        case "references":
                        {
                            header.References = this.ParseReferences(str);
                            continue;
                        }
                        case "subject":
                        {
                            header.Subject = this.ParseSubject(str);
                            continue;
                        }
                        case "comments":
                        {
                            header.Comments = this.ParseComments(str);
                            continue;
                        }
                        case "keywords":
                        {
                            header.Keywords = this.ParseKeywords(str);
                            continue;
                        }
                        case "content-disposition":
                        {
                            header.ContentDisposition = this.ParseContentDisposition(str);
                            continue;
                        }
                        case "content-id":
                        {
                            header.ContentID = this.ParseContentID(str);
                            continue;
                        }
                        case "resent-date":
                        {
                            header.ResentDate = this.ParseDate(str);
                            continue;
                        }
                        case "resent-from":
                        {
                            header.ResentFrom = this.ParseFrom(str);
                            continue;
                        }
                        case "resent-sender":
                        {
                            header.ResentSender = this.ParseSender(str);
                            continue;
                        }
                        case "resent-to":
                        {
                            header.ResentTo = this.ParseTo(str);
                            continue;
                        }
                        case "resent-cc":
                        {
                            header.ResentCarbonCopies = this.ParseCarbonCopies(str);
                            continue;
                        }
                        case "resent-bcc":
                        {
                            header.ResentBlindedCarbonCopies = this.ParseBlindCarbonCopies(str);
                            continue;
                        }
                        case "resent-message-id":
                        {
                            header.ResentMessageID = this.ParseMessageID(str);
                            continue;
                        }
                        case "return-path":
                        {
                            header.ReturnPath = this.ParseReturnPatch(str);
                            continue;
                        }
                        case "received":
                        {
                            if (string.IsNullOrEmpty(header.Received))
                            {
                                header.Received = str.Substring(9).Trim();
                            }
                            continue;
                        }
                        case "content-type":
                        {
                            header.ContentType = this.ParseContentType(str);
                            continue;
                        }
                        case "content-transfer-encoding":
                        {
                            header.ContentTransferEncoding = this.ParseContentTransferEncoding(str);
                            continue;
                        }
                        case "content-description":
                        {
                            header.ContentDescription = this.ParseContentDescription(str);
                            continue;
                        }           
						case "importance":
                        {
                            header.Importance = this.ParseImportance(str);
                            continue;
                        }
                    }
                    DictionaryExtenders.SmartAdd(header.ExtraHeaders, match.Groups[1].Value, match.Groups[2].Value);
                }
            }
            return header;
        }

        public EmailAddressCollection ParseBlindCarbonCopies(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            EmailAddressCollection addresss = new EmailAddressCollection();
            Match match = regBCC.Match(text);
            if (match.Success && (match.Groups.Count > 0))
            {
                foreach (Match match2 in regAddressAll.Matches(match.Groups[0].Value))
                {
                    EmailAddress address = this.ParseEmail(match2.Groups[1].Value);
                    if (address != null)
                    {
                        addresss.Add(address);
                    }
                }
                return addresss;
            }
            foreach (Match match3 in regAddressAll.Matches(text))
            {
                EmailAddress address2 = this.ParseEmail(match3.Groups[1].Value);
                if (address2 != null)
                {
                    addresss.Add(address2);
                }
            }
            return addresss;
        }

        public EmailAddressCollection ParseCarbonCopies(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            EmailAddressCollection addresss = new EmailAddressCollection();
            Match match = regCC.Match(text);
            if (match.Success && (match.Groups.Count > 0))
            {
                foreach (Match match2 in regAddressAll.Matches(match.Groups[0].Value))
                {
                    EmailAddress address = this.ParseEmail(match2.Groups[1].Value);
                    if (address != null)
                    {
                        addresss.Add(address);
                    }
                }
                return addresss;
            }
            foreach (Match match3 in regAddressAll.Matches(text))
            {
                EmailAddress address2 = this.ParseEmail(match3.Groups[1].Value);
                if (address2 != null)
                {
                    addresss.Add(address2);
                }
            }
            return addresss;
        }

        public string ParseComments(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Match match = regComments.Match(text);
            if (!match.Success || (match.Groups.Count <= 1))
            {
                return null;
            }
            if (!regSubjectItem.IsMatch(match.Groups[1].Value))
            {
                return match.Groups[1].Value;
            }
            return MailMessageRFCDecoder.ParseBase64Item(match.Groups[1].Value);
        }

        public string ParseContentDescription(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Match match = regContentDescription.Match(text);
            if (match.Success && (match.Groups.Count > 1))
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public ContentDisposition ParseContentDisposition(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            ContentDisposition disposition = new ContentDisposition();
            Match match = regContentDisposition.Match(text);
            if (!match.Success || (match.Groups.Count <= 0))
            {
                return null;
            }
            MatchCollection matchs = regKeyValue.Matches(match.Value);
            for (int i = 0; i < matchs.Count; i++)
            {
                string key = matchs[i].Groups[1].Value.ToLower();
                string input = matchs[i].Groups[2].Value;
                Match match2 = regDifArgumentValue.Match(input);
                if (match2.Success && (match2.Groups.Count > 1))
                {
                    input = match2.Groups[1].Value;
                }
                DictionaryExtenders.SmartAdd(disposition.Attributes, key, input);
            }
            disposition.Disposition = match.Groups[1].Value;
            return disposition;
        }

        public string ParseContentID(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Match match = regContentID.Match(text);
            if (match.Success && (match.Groups.Count > 1))
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public EContentTransferEncoding ParseContentTransferEncoding(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (this.IsValidCharacters(text))
            {
                Match match = regContentTransferEncoding.Match(text);
                if (match.Success && (match.Groups.Count > 1))
                {
                    return GetTransferEncodingFromString(match.Groups[1].Value.ToLowerInvariant());
                }
            }
            return EContentTransferEncoding.SevenBit;
        }

        public ContentType ParseContentType(string text)
        {
            ContentType type;
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (!this.IsValidCharacters(text))
            {
                type = new ContentType();
                type.Attributes.Add("charset", "us-ascii");
                return type;
            }
            Match match = regContentType.Match(text);
            if (match.Success && (match.Groups.Count > 2))
            {
                type = new ContentType(match.Groups[1].Value, match.Groups[2].Value);
                MatchCollection matchs = regKeyValue.Matches(match.Value);
                for (int i = 0; i < matchs.Count; i++)
                {
                    DictionaryExtenders.SmartAdd(type.Attributes, matchs[i].Groups[1].Value.ToLower().Trim(), matchs[i].Groups[2].Value.Trim());
                }
                if ((type.Type == "text") && !type.Attributes.ContainsKey("charset"))
                {
                    type.Attributes.Add("charset", "us-ascii");
                }
                return type;
            }
            type = new ContentType();
            type.Attributes.Add("charset", "us-ascii");
            return type;
        }

        public DateTime ParseDate(string text)
        {
            DateTime time;
            short num;
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Skycap.Net.Common.DateTimeOffset minValue = Skycap.Net.Common.DateTimeOffset.MinValue;
            Match match = regDate.Match(text);
            if ((match.Success && (match.Groups.Count > 2)) && DateTime.TryParse(match.Groups[2].Value, out time))
            {
                minValue = new Skycap.Net.Common.DateTimeOffset(time);
            }
            if ((match.Success && (match.Groups.Count > 3)) && short.TryParse(match.Groups[3].Value, out num))
            {
                float num2 = ((float) num) / 100f;
                minValue = new Net.Common.DateTimeOffset(minValue.DateTime, new TimeSpan((int) num2, num - (((int) num2) * 100), 0));
            }
            if (!match.Success)
            {
                if (text.Contains(","))
                {
                    text = text.Replace(" (SAST)", "");
                    if (DateTime.TryParse(text.Split(',')[1], out time))
                    return time;
                }
            }
            return minValue.UtcDateTime;
        }

        public EmailAddress ParseEmail(string text)
        {
            string str;
            EmailAddress address;
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            string input = "";
            Match match = regAddressDifDisplayName.Match(text);
            if (match.Success && (match.Groups.Count > 1))
            {
                input = match.Groups[1].Value;
                str = match.Groups[2].Value;
            }
            else
            {
                match = regAddressDisplayName.Match(text);
                if (match.Success && (match.Groups.Count > 1))
                {
                    input = match.Groups[1].Value;
                    str = match.Groups[2].Value;
                }
                else
                {
                    match = regAddressEasy.Match(text);
                    if (match.Success && (match.Groups.Count > 1))
                    {
                        str = match.Groups[1].Value;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            if (regSubjectItem.IsMatch(input))
            {
                input = MailMessageRFCDecoder.ParseBase64Item(input);
            }
            try
            {
                address = new EmailAddress(str, input.Trim());
            }
            catch
            {
                return null;
            }
            return address;
        }

        public EmailAddress ParseFrom(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Match match = regFrom.Match(text);
            if (match.Success && (match.Groups.Count > 2))
            {
                return this.ParseEmail(match.Groups[1].Value);
            }
            match = regEasyFrom.Match(text);
            if (match.Success && (match.Groups.Count > 1))
            {
                return this.ParseEmail(match.Groups[1].Value);
            }
            return null;
        }

        public string ParseInReplyTo(string header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            Match match = regInReplyTo.Match(header);
            if (match.Success && (match.Groups.Count > 1))
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public IList<string> ParseKeywords(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            List<string> list = new List<string>();
            Match match = regKeywords.Match(text);
            if (match.Success && (match.Groups.Count > 1))
            {
                foreach (Match match2 in regKeyword.Matches(match.Groups[1].Value))
                {
                    list.Add(regSubjectItem.IsMatch(match2.Groups[1].Value) ? MailMessageRFCDecoder.ParseBase64Item(match2.Groups[1].Value) : match2.Groups[1].Value);
                }
            }
            return list;
        }

        public string ParseMessageID(string header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            Match match = regMessageID.Match(header);
            if (match.Success && (match.Groups.Count > 1))
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public Version ParseMimeVersion(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }
            Version version = new Version(1, 0);
            Match match = regMIMEVersion.Match(line);
            if (match.Success && (match.Groups.Count > 1))
            {
                version = new Version(match.Groups[1].Value);
            }
            return version;
        }

        public ExtraHeadersDictionary ParseMIMEXAtribbutes(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            ExtraHeadersDictionary dictionary = new ExtraHeadersDictionary();
            MatchCollection matchs = regXAtribbutes.Matches(text);
            for (int i = 0; i < matchs.Count; i++)
            {
                Match match = matchs[i];
                if (match.Groups.Count > 2)
                {
                    dictionary.Add(match.Groups[1].Value, match.Groups[2].Value);
                }
            }
            return dictionary;
        }

        public string ParseReferences(string header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            Match match = regReferences.Match(header);
            if (match.Success && (match.Groups.Count > 1))
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        public EmailAddressCollection ParseReplyTo(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            EmailAddressCollection addresss = new EmailAddressCollection();
            Match match = regReplyTo.Match(text);
            if (match.Success && (match.Groups.Count > 0))
            {
                foreach (Match match2 in regAddressAll.Matches(match.Groups[0].Value))
                {
                    EmailAddress address = this.ParseEmail(match2.Groups[1].Value);
                    if (address != null)
                    {
                        addresss.Add(address);
                    }
                }
            }
            return addresss;
        }

        public EmailAddress ParseReturnPatch(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }
            if (line.Length > 13)
            {
                line = line.Remove(0, 13);
            }
            return this.ParseEmail(line);
        }

        public EmailAddress ParseSender(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Match match = regSender.Match(text);
            if (match.Success && (match.Groups.Count > 1))
            {
                return this.ParseEmail(match.Groups[1].Value);
            }
            return null;
        }

        public string ParseSubject(string header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            StringBuilder builder = new StringBuilder("");
            Match match = regSubject.Match(header);
            if (match.Success && (match.Groups.Count > 0))
            {
                MatchCollection matchs = regSubjectItem.Matches(match.Groups[1].Value);
                if (matchs.Count == 0)
                {
                    builder = new StringBuilder(match.Groups[1].Value);
                }
                else
                {
                    foreach (Match match2 in matchs)
                    {
                        builder.Append(MailMessageRFCDecoder.ParseBase64Item(match2.Value));
                    }
                }
            }
            return builder.ToString();
        }

        public EmailAddressCollection ParseTo(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            EmailAddressCollection addresss = new EmailAddressCollection();
            Match match = regTo.Match(text);
            if (match.Success && (match.Groups.Count > 0))
            {
                foreach (Match match2 in regAddressAll.Matches(match.Groups[0].Value))
                {
                    EmailAddress address = this.ParseEmail(match2.Groups[1].Value);
                    if (address != null)
                    {
                        addresss.Add(address);
                    }
                }
                return addresss;
            }
            foreach (Match match3 in regAddressAll.Matches(text))
            {
                EmailAddress address2 = this.ParseEmail(match3.Groups[1].Value);
                if (address2 != null)
                {
                    addresss.Add(address2);
                }
            }
            return addresss;
        }

        public MailImportance ParseImportance(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            Match match = regImportance.Match(text);
            if (match.Success && (match.Groups.Count > 1))
                return (MailImportance)Enum.Parse(typeof(MailImportance), match.Groups[1].Value, true);
            return MailImportance.Normal;
        }
    }
}

