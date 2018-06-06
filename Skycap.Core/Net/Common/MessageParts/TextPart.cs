namespace Skycap.Net.Common.MessageParts
{
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
    public class TextPart : BaseContentPart
    {
        protected string _text;

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
        public override EPartType Type
        {
            get
            {
                return EPartType.Text;
            }
        }
    }
}

