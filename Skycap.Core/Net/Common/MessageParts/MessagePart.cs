namespace Skycap.Net.Common.MessageParts
{
    using Skycap.Net.Common;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(Attachment))]
    [KnownType(typeof(BaseContentPart))]
    [KnownType(typeof(ContentPart))]
    [KnownType(typeof(MessagePart))]
    [KnownType(typeof(MultiPart))]
    [KnownType(typeof(TextPart))]
    [KnownType(typeof(MessageDeliveryStatusPart))]
    [KnownType(typeof(MessageDispositionNotificationPart))]
    public class MessagePart : IPart
    {
        protected MIMEHeader _header = new MIMEHeader();
        protected MailMessage _message;

        [DataMember]
        public virtual MIMEHeader Header
        {
            get
            {
                return this._header;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._header = value;
            }
        }

        [DataMember]
        public virtual MailMessage Message
        {
            get
            {
                return this._message;
            }
            set
            {
                this._message = value;
            }
        }

        [IgnoreDataMember]
        public virtual EPartType Type
        {
            get
            {
                return EPartType.Message;
            }
        }
    }
}

