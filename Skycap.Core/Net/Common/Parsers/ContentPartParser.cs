namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.ContentWriters;
    using Skycap.Net.Common.MessageParts;
    using System;

    public class ContentPartParser : BaseContentParser
    {
        protected FileContentWriter _contentWriter;

        public ContentPartParser(string attachmentDirectory)
        {
            this._contentWriter = new FileContentWriter(attachmentDirectory);
        }

        protected override IPart CreatePart(ContentType contentType, ContentDisposition contentDisposition)
        {
            return new ContentPart { TransferFilename = PartUtils.ExtractOriginalFilename(contentType, contentDisposition), DiskFilename = this._contentWriter.Filename, Size = this._contentWriter.Size };
        }

        public override IContentWriter ContentWriter
        {
            get
            {
                return this._contentWriter;
            }
            protected set
            {
                this._contentWriter = (FileContentWriter) value;
            }
        }
    }
}

