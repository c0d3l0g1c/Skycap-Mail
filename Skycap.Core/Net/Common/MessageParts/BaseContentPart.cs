namespace Skycap.Net.Common.MessageParts
{
    using Skycap.Net.Common;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [DataContract]
    [DebuggerDisplay("{ContentType}")]
    [KnownType(typeof(Attachment))]
    [KnownType(typeof(BaseContentPart))]
    [KnownType(typeof(ContentPart))]
    [KnownType(typeof(MessagePart))]
    [KnownType(typeof(MultiPart))]
    [KnownType(typeof(TextPart))]
    [KnownType(typeof(MessageDeliveryStatusPart))]
    [KnownType(typeof(MessageDispositionNotificationPart))]
    public abstract class BaseContentPart : IPart
    {
        protected MIMEHeader _header = new MIMEHeader();

        protected BaseContentPart()
        {
        }

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

        [IgnoreDataMember]
        public abstract EPartType Type { get; }
    }
}

