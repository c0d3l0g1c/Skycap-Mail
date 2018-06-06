namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Common.MessageReaders;
    using System;
    using Skycap.IO;
    using System.Threading.Tasks;
    using System.IO;

    public abstract class BaseComplexPartParser : BaseParser
    {
        protected BaseComplexPartParser()
        {
        }

        protected abstract void childParser_FinalBoundaryReached(object sender, EventArgs e);
        public virtual IPart ParsePart(IMessageReader reader, string contentId, ContentType contentType, ContentDisposition contentDisposition, EContentTransferEncoding contentTransferEncoding, string uid, string attachmentDirectory, string boundary, PopMessage parentMessage)
        {
            if (PartUtils.IsMultipart(contentType))
            {
                MultiPartParser parser = new MultiPartParser();
                parser.FinalBoundaryReached += new EventHandler(this.childParser_FinalBoundaryReached);
                return parser.Parse(reader, contentTransferEncoding, boundary, uid, attachmentDirectory, parentMessage, contentType);
            }
            if (PartUtils.IsTextPart(contentType, contentDisposition))
            {
                string charset = contentType.Attributes.ContainsKey("charset") ? contentType.Attributes["charset"] : "us-ascii";
                TextPartParser parser2 = new TextPartParser(charset);
                parser2.FinalBoundaryReached += new EventHandler(this.childParser_FinalBoundaryReached);
                return parser2.Parse(reader, contentType, contentDisposition, contentTransferEncoding, boundary);
            }
            if (PartUtils.IsMessagePart(contentType) && (contentType.SubType == "delivery-status"))
            {
                MessageDeliveryStatusPartParser parser3 = new MessageDeliveryStatusPartParser();
                parser3.FinalBoundaryReached += new EventHandler(this.childParser_FinalBoundaryReached);
                return parser3.Parse(reader, contentType, contentDisposition, contentTransferEncoding, boundary);
            }
            if (PartUtils.IsMessagePart(contentType) && (contentType.SubType == "disposition-notification"))
            {
                MessageDispositionNotificationPartParser parser4 = new MessageDispositionNotificationPartParser();
                parser4.FinalBoundaryReached += new EventHandler(this.childParser_FinalBoundaryReached);
                return parser4.Parse(reader, contentType, contentDisposition, contentTransferEncoding, boundary);
            }
            if (PartUtils.IsMessagePart(contentType))
            {
                MessagePartParser parser5 = new MessagePartParser();
                parser5.FinalBoundaryReached += new EventHandler(this.childParser_FinalBoundaryReached);
                return parser5.Parse(reader, contentType, contentDisposition, contentTransferEncoding, boundary, uid, attachmentDirectory);
            }
            attachmentDirectory = Path.Combine(attachmentDirectory, MailClient.MessageFolderPrefix + IOUtil.FormatFileSystemName(uid));
            ContentPartParser parser6 = new ContentPartParser(attachmentDirectory);
            parser6.FinalBoundaryReached += new EventHandler(this.childParser_FinalBoundaryReached);
            ContentPart part2 = (ContentPart) parser6.Parse(reader, contentType, contentDisposition, contentTransferEncoding, boundary);
            IPart part = part2;
            Attachment attachment = new Attachment(part2.DiskFilename, contentId, contentType, attachmentDirectory, part2.Size) {
                DiskFilename = part2.DiskFilename,
                TransferFilename = part2.TransferFilename,
                AttachmentDirectory = attachmentDirectory,
                ContentID = contentId,
                ContentType = contentType
            };
            if (!attachment.TransferFilenameExtension.Equals(".octet-stream", StringComparison.OrdinalIgnoreCase)) parentMessage.Attachments.Add(attachment);
            return part;
        }
    }
}

