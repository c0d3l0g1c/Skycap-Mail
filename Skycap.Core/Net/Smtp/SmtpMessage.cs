namespace Skycap.Net.Smtp
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Collections;
    using Skycap.Net.Common.MessageParts;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Skycap.IO;
    using Windows.Storage;

    public class SmtpMessage : MailMessage
    {
        protected Encoding _textEncoding;
        public const string DefaultEncodingWebName = "UTF-8";

        protected SmtpMessage()
        {
            this.Init();
        }

        public SmtpMessage(MailMessage message, Encoding encoding)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            this.Header = message.Header;
            this.Text = message.Text;
            this.TextEncoding = encoding;
            this.PlainText = message.PlainText;
            this.TextContentType = message.TextContentType;
            this.Attachments.AddRange(message.Attachments);
        }

        public SmtpMessage(EmailAddress from, EmailAddressCollection to, string subject, string messageText)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }
            if (messageText == null)
            {
                throw new ArgumentNullException("messageText");
            }
            this.Init();
            this.From = from;
            this.To = new EmailAddressCollection(to);
            this.Subject = subject;
            this.Text = messageText;
        }

        public SmtpMessage(EmailAddress from, EmailAddress to, string subject, string messageText)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }
            if (messageText == null)
            {
                throw new ArgumentNullException("messageText");
            }
            this.Init();
            this.From = from;
            this.To.Add(to);
            this.Subject = subject;
            this.Text = messageText;
        }

        public virtual EMessageCheckResult CheckCorrectness()
        {
            if ((this.TextContentType == ETextContentType.Html) && string.IsNullOrEmpty(this.PlainText))
            {
                return EMessageCheckResult.NoPlainTextInHTMLMessage;
            }
            if (this.From == null)
            {
                return EMessageCheckResult.NoFromField;
            }
            if ((this.To == null) || (this.To.Count == 0))
            {
                return EMessageCheckResult.NoToField;
            }
            if (this.Text == null)
            {
                return EMessageCheckResult.NoText;
            }
            foreach (Attachment attachment in this.Attachments)
            {
                if (IOUtil.FileExists(attachment.FullFilename) == null)
                {
                    return EMessageCheckResult.AttachmentFileIsMissing;
                }
            }
            return EMessageCheckResult.Correct;
        }

        protected virtual void Init()
        {
            this._textEncoding = Encoding.GetEncoding("UTF-8");
        }

        public virtual string Comments
        {
            get
            {
                return this.Header.Comments;
            }
            set
            {
                this.Header.Comments = value;
            }
        }

        public virtual ExtraHeadersDictionary ExtraHeaders
        {
            get
            {
                return this.Header.ExtraHeaders;
            }
        }

        public virtual string InReplyTo
        {
            get
            {
                return this.Header.InReplyTo;
            }
            set
            {
                this.Header.InReplyTo = value;
            }
        }

        public virtual IList<string> Keywords
        {
            get
            {
                return this.Header.Keywords;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Header.Keywords = value;
            }
        }

        public virtual string Received
        {
            get
            {
                return this.Header.Received;
            }
            set
            {
                this.Header.Received = value;
            }
        }

        public virtual string References
        {
            get
            {
                return this.Header.References;
            }
            set
            {
                this.Header.References = value;
            }
        }

        public virtual EmailAddressCollection ReplyTo
        {
            get
            {
                return this.Header.ReplyTo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Header.ReplyTo = value;
            }
        }

        public virtual EmailAddressCollection ResentBlindedCarbonCopies
        {
            get
            {
                return this.Header.ResentCarbonCopies;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Header.ResentBlindedCarbonCopies = value;
            }
        }

        public virtual EmailAddressCollection ResentCarbonCopies
        {
            get
            {
                return this.Header.ResentCarbonCopies;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Header.ResentCarbonCopies = value;
            }
        }

        public virtual DateTime ResentDate
        {
            get
            {
                return this.Header.ResentDate;
            }
            set
            {
                this.Header.ResentDate = value;
            }
        }

        public virtual EmailAddress ResentFrom
        {
            get
            {
                return this.Header.ResentFrom;
            }
            set
            {
                this.Header.ResentFrom = value;
            }
        }

        public virtual string ResentMessageID
        {
            get
            {
                return this.Header.ResentMessageID;
            }
            set
            {
                this.Header.ResentMessageID = value;
            }
        }

        public virtual EmailAddress ResentSender
        {
            get
            {
                return this.Header.ResentSender;
            }
            set
            {
                this.Header.ResentSender = value;
            }
        }

        public virtual EmailAddressCollection ResentTo
        {
            get
            {
                return this.Header.ResentTo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Header.ResentTo = value;
            }
        }

        public virtual EmailAddress Sender
        {
            get
            {
                return this.Header.Sender;
            }
            set
            {
                this.Header.Sender = value;
            }
        }

        public virtual Encoding TextEncoding
        {
            get
            {
                return this._textEncoding;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._textEncoding = value;
            }
        }
    }
}

