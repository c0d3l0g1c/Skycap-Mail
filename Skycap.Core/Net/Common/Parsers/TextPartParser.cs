namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.ContentWriters;
    using Skycap.Net.Common.MessageParts;
    using System;

    public class TextPartParser : BaseContentParser
    {
        protected StringContentWriter _contentWriter;

        public TextPartParser(string charset)
        {
            this._contentWriter = new StringContentWriter(charset);
        }

        protected override IPart CreatePart(ContentType contentType, ContentDisposition contentDisposition)
        {
            return new TextPart { Text = this._contentWriter.Text };
        }

        public override IContentWriter ContentWriter
        {
            get
            {
                return this._contentWriter;
            }
            protected set
            {
                this._contentWriter = (StringContentWriter) value;
            }
        }
    }
}

