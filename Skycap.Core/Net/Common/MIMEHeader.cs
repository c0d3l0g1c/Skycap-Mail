namespace Skycap.Net.Common
{
    using Skycap.Net.Common.Collections;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;

    [DataContract]
    public class MIMEHeader
    {
        private EmailAddressCollection _blindedCarbonCopies;
        private EmailAddressCollection _carbonCopies;
        private string _comments;
        private string _contentDescription;
        private Net.Common.ContentDisposition _contentDisposition;
        private string _contentId;
        private EContentTransferEncoding _contentTransferEncoding;
        private Net.Common.ContentType _contentType;
        private DateTime _date;
        protected const EContentTransferEncoding _defaultContentTransferEncoding = EContentTransferEncoding.SevenBit;
        private ExtraHeadersDictionary _extraHeaders;
        private EmailAddress _from;
        private string _inReplyTo;
        private bool _isDefaultContentTransferEncoding;
        private bool _isDefaultContentType;
        private IList<string> _keywords;
        private string _messageId;
        private string _received;
        private string _references;
        private EmailAddressCollection _replyTo;
        private EmailAddressCollection _resentBlindedCarbonCopies;
        private EmailAddressCollection _resentCarbonCopies;
        private DateTime _resentDate;
        private EmailAddress _resentFrom;
        private string _resentMessageId;
        private EmailAddress _resentSender;
        private EmailAddressCollection _resentTo;
        private EmailAddress _returnPath;
        private EmailAddress _sender;
        private string _subject;
        private string _text;
        private Encoding _textEncoding;
        private EmailAddressCollection _to;
        private MailImportance _importance;

        public MIMEHeader()
        {
            Initialise();
        }

        [DataMember]
        public virtual EmailAddressCollection BlindedCarbonCopies
        {
            get
            {
                return this._blindedCarbonCopies;
            }
            set
            {
                this._blindedCarbonCopies = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection CarbonCopies
        {
            get
            {
                return this._carbonCopies;
            }
            set
            {
                this._carbonCopies = value;
            }
        }

        [DataMember]
        public virtual string Comments
        {
            get
            {
                return this._comments;
            }
            set
            {
                this._comments = value;
            }
        }

        [DataMember]
        public virtual string ContentDescription
        {
            get
            {
                return this._contentDescription;
            }
            set
            {
                this._contentDescription = value;
            }
        }

        [DataMember]
        public Net.Common.ContentDisposition ContentDisposition
        {
            get
            {
                return this._contentDisposition;
            }
            set
            {
                this._contentDisposition = value;
            }
        }

        [DataMember]
        public virtual string ContentID
        {
            get
            {
                return this._contentId;
            }
            set
            {
                this._contentId = value;
            }
        }

        [DataMember]
        public virtual EContentTransferEncoding ContentTransferEncoding
        {
            get
            {
                return this._contentTransferEncoding;
            }
            set
            {
                this._contentTransferEncoding = value;
                this._isDefaultContentTransferEncoding = false;
            }
        }

        [DataMember]
        public virtual Net.Common.ContentType ContentType
        {
            get
            {
                return this._contentType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._contentType = value;
                this._isDefaultContentType = false;
            }
        }

        [DataMember]
        public virtual DateTime Date
        {
            get
            {
                return this._date;
            }
            set
            {
                this._date = value;
            }
        }

        [DataMember]
        public ExtraHeadersDictionary ExtraHeaders
        {
            get
            {
                return this._extraHeaders;
            }
        }

        [DataMember]
        public virtual EmailAddress From
        {
            get
            {
                return this._from;
            }
            set
            {
                this._from = value;
            }
        }

        [DataMember]
        public virtual string InReplyTo
        {
            get
            {
                return this._inReplyTo;
            }
            set
            {
                this._inReplyTo = value;
            }
        }

        [DataMember]
        public bool IsDefaultContentTransferEncoding
        {
            get
            {
                return this._isDefaultContentTransferEncoding;
            }
            internal set
            {
                this._isDefaultContentTransferEncoding = value;
            }
        }

        [DataMember]
        public bool IsDefaultContentType
        {
            get
            {
                return this._isDefaultContentType;
            }
            internal set
            {
                this._isDefaultContentType = value;
            }
        }

        [DataMember]
        public virtual IList<string> Keywords
        {
            get
            {
                return this._keywords;
            }
            set
            {
                this._keywords = value;
            }
        }

        [DataMember]
        public virtual string MessageID
        {
            get
            {
                return this._messageId;
            }
            set
            {
                this._messageId = value;
            }
        }

        [DataMember]
        public virtual string Received
        {
            get
            {
                return this._received;
            }
            set
            {
                this._received = value;
            }
        }

        [DataMember]
        public virtual string References
        {
            get
            {
                return this._references;
            }
            set
            {
                this._references = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection ReplyTo
        {
            get
            {
                return this._replyTo;
            }
            set
            {
                this._replyTo = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection ResentBlindedCarbonCopies
        {
            get
            {
                return this._resentBlindedCarbonCopies;
            }
            set
            {
                this._resentBlindedCarbonCopies = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection ResentCarbonCopies
        {
            get
            {
                return this._resentCarbonCopies;
            }
            set
            {
                this._resentCarbonCopies = value;
            }
        }

        [DataMember]
        public virtual DateTime ResentDate
        {
            get
            {
                return this._resentDate;
            }
            set
            {
                this._resentDate = value;
            }
        }

        [DataMember]
        public virtual EmailAddress ResentFrom
        {
            get
            {
                return this._resentFrom;
            }
            set
            {
                this._resentFrom = value;
            }
        }

        [DataMember]
        public virtual string ResentMessageID
        {
            get
            {
                return this._resentMessageId;
            }
            set
            {
                this._resentMessageId = value;
            }
        }

        [DataMember]
        public virtual EmailAddress ResentSender
        {
            get
            {
                return this._resentSender;
            }
            set
            {
                this._resentSender = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection ResentTo
        {
            get
            {
                return this._resentTo;
            }
            set
            {
                this._resentTo = value;
            }
        }

        [DataMember]
        public virtual EmailAddress ReturnPath
        {
            get
            {
                return this._returnPath;
            }
            set
            {
                this._returnPath = value;
            }
        }

        [DataMember]
        public virtual EmailAddress Sender
        {
            get
            {
                return this._sender;
            }
            set
            {
                this._sender = value;
            }
        }

        [DataMember]
        public virtual string Subject
        {
            get
            {
                return this._subject;
            }
            set
            {
                this._subject = value;
            }
        }

        [DataMember]
        public virtual string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                this._text = value;
            }
        }

        [IgnoreDataMember]
        public virtual Encoding TextEncoding
        {
            get
            {
                return this._textEncoding;
            }
            set
            {
                this._textEncoding = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection To
        {
            get
            {
                return this._to;
            }
            set
            {
                this._to = value;
            }
        }

        [DataMember]
        public MailImportance Importance
        {
            get
            {
                return this._importance;
            }
            set
            { 
                this._importance = value;
            }
        }

        private void Initialise()
        {
            this._blindedCarbonCopies = new EmailAddressCollection();
            this._carbonCopies = new EmailAddressCollection();
            this._contentTransferEncoding = EContentTransferEncoding.SevenBit;
            this._contentType = new Net.Common.ContentType();
            this._extraHeaders = new ExtraHeadersDictionary();
            this._isDefaultContentTransferEncoding = true;
            this._isDefaultContentType = true;
            this._keywords = new List<string>();
            this._replyTo = new EmailAddressCollection();
            this._resentBlindedCarbonCopies = new EmailAddressCollection();
            this._resentCarbonCopies = new EmailAddressCollection();
            this._resentTo = new EmailAddressCollection();
            this._comments = string.Empty;
            this._contentDescription = string.Empty;
            this._contentId = string.Empty;
            this._inReplyTo = string.Empty;
            this._messageId = string.Empty;
            this._received = string.Empty;
            this._references = string.Empty;
            this._resentMessageId = string.Empty;
            this._subject = string.Empty;
            this._text = string.Empty;
            this._textEncoding = Encoding.UTF8;
            this._to = new EmailAddressCollection();
            this._importance = MailImportance.Normal;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            Initialise();
        }
    }
}

