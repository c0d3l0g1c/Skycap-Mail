namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Exceptions;
    using Skycap.Net.Common.MessageReaders;
    using System;
    using System.Collections.Generic;
    using Windows.Storage;

    public class MailMessageParser : BaseComplexPartParser
    {
        protected override void childParser_FinalBoundaryReached(object sender, EventArgs e)
        {
        }

        public PopMessage Parse(IMessageReader reader, string uid, string attachmentDirectory)
        {
            PopMessage message;
            MIMEHeader header;
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (attachmentDirectory == null)
            {
                throw new ArgumentNullException("attachmentDirectory");
            }
            try
            {
                MIMEHeaderParser parser = new MIMEHeaderParser();
                message = new PopMessage();
                message.Uid = uid;
                string source = Utils.ReadHeaders(reader);
                header = parser.Parse(source);
                message.Date = header.Date;
                message.From = header.From;
                message.Header.Sender = header.Sender;
                message.Header.ReplyTo = header.ReplyTo;
                message.To = header.To;
                message.CarbonCopies = header.CarbonCopies;
                message.BlindedCarbonCopies = header.BlindedCarbonCopies;
                message.Header.MessageID = header.MessageID;
                message.Header.InReplyTo = header.InReplyTo;
                message.Header.References = header.References;
                message.Subject = header.Subject;
                message.Header.Comments = header.Comments;
                message.Header.Keywords = header.Keywords;
                message.Header.ContentDisposition = header.ContentDisposition;
                message.Header.ResentDate = header.ResentDate;
                message.Header.ResentFrom = header.ResentFrom;
                message.Header.ResentSender = header.ResentSender;
                message.Header.ResentTo = header.ResentTo;
                message.Header.ResentCarbonCopies = header.ResentCarbonCopies;
                message.Header.ResentBlindedCarbonCopies = header.ResentBlindedCarbonCopies;
                message.Header.ResentMessageID = header.ResentMessageID;
                message.Header.ReturnPath = header.ReturnPath;
                message.Header.Received = header.Received;
                message.Header.ContentType = header.ContentType;
                message.Header.ContentTransferEncoding = header.ContentTransferEncoding;
                foreach (KeyValuePair<string, string> pair in header.ExtraHeaders)
                {
                    message.Header.ExtraHeaders.SmartAdd(pair.Key, pair.Value);
                }
                if (!reader.EndOfMessage)
                {
                    message.RootPart = this.ParsePart(reader, message.Header.ContentID, message.Header.ContentType, message.Header.ContentDisposition, message.Header.ContentTransferEncoding, uid, attachmentDirectory, null, message);
                }
            }
            finally
            {
                while (!reader.EndOfMessage)
                {
                    reader.ReadLine();
                }
            }
            if (message.RootPart != null)
            {
                message.RootPart.Header = header;
                message.FillText();
            }
            return message;
        }
    }
}

