namespace Skycap.Net.Common.MessageParts
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Collections;
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
    public class MultiPart : IPart
    {
        protected MIMEHeader _header = new MIMEHeader();
        protected PartCollection _parts = new PartCollection();

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
        public virtual PartCollection Parts
        {
            get
            {
                return this._parts;
            }
            protected set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._parts = value;
            }
        }

        [IgnoreDataMember]
        public virtual EPartType Type
        {
            get
            {
                return EPartType.Multi;
            }
        }
    }
}

