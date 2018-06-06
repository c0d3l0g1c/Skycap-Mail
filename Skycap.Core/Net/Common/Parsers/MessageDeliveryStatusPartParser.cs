namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.MessageParts;
    using System;

    public class MessageDeliveryStatusPartParser : TextPartParser
    {
        public MessageDeliveryStatusPartParser() : base("us-ascii")
        {
        }

        protected override IPart CreatePart(ContentType contentType, ContentDisposition contentDisposition)
        {
            return new MessageDeliveryStatusPart { Text = base._contentWriter.Text };
        }
    }
}

