namespace Skycap.Net.Common.MessageParts
{
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [DataContract]
    [DebuggerDisplay("{Type}")]
    [KnownType(typeof(Attachment))]
    [KnownType(typeof(BaseContentPart))]
    [KnownType(typeof(ContentPart))]
    [KnownType(typeof(MessagePart))]
    [KnownType(typeof(MultiPart))]
    [KnownType(typeof(TextPart))]
    [KnownType(typeof(MessageDeliveryStatusPart))]
    [KnownType(typeof(MessageDispositionNotificationPart))]
    public class MessageDispositionNotificationPart : TextPart
    {
        [IgnoreDataMember]
        public override EPartType Type
        {
            get
            {
                return EPartType.Message;
            }
        }
    }
}

