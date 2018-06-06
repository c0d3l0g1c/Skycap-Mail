namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Common.MessageReaders;
    using System;

    public class MessagePartParser : BaseParser
    {
        public IPart Parse(IMessageReader reader, ContentType contentType, ContentDisposition contentDisposition, EContentTransferEncoding contentTransferEncoding, string boundary, string uid, string attachmentDirectory)
        {
            BoundedMessageReader reader2;
            MessagePart part = new MessagePart();
            if (contentTransferEncoding == EContentTransferEncoding.Base64)
            {
                reader2 = new Base64MessageReader(reader, boundary);
            }
            else if (contentTransferEncoding == EContentTransferEncoding.QuotedPrintable)
            {
                reader2 = new QuotedPrintableMessageReader(reader, boundary);
            }
            else
            {
                reader2 = new UnencodedMessageReader(reader, boundary);
            }
            PopMessage message = new MailMessageParser().Parse(reader2, uid, attachmentDirectory);
            part.Message = message;
            if (reader2.FinalBoundaryReached)
            {
                this.RaiseFinalBoundaryReached();
            }
            return part;
        }
    }
}

