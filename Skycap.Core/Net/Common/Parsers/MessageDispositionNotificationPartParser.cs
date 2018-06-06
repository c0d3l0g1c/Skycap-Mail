namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.MessageParts;
    using System;

    public class MessageDispositionNotificationPartParser : TextPartParser
    {
        public MessageDispositionNotificationPartParser() : base("us-ascii")
        {
        }

        protected override IPart CreatePart(ContentType contentType, ContentDisposition contentDisposition)
        {
            return new MessageDispositionNotificationPart { Text = base._contentWriter.Text };
        }
    }
}

