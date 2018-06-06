namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.ContentWriters;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Common.MessageReaders;
    using System;

    public abstract class BaseContentParser : BaseParser
    {
        protected BaseContentParser()
        {
        }

        protected abstract IPart CreatePart(ContentType contentType, ContentDisposition contentDisposition);
        public virtual IPart Parse(IMessageReader reader, ContentType contentType, ContentDisposition contentDisposition, EContentTransferEncoding contentTransferEncoding, string boundary)
        {
            this.ContentWriter.Open();
            if (contentTransferEncoding == EContentTransferEncoding.Base64)
            {
                this.ParseFromBase64(reader, contentType, contentDisposition, boundary);
            }
            else if (contentTransferEncoding == EContentTransferEncoding.QuotedPrintable)
            {
                this.ParseFromQuotedPrintable(reader, contentType, contentDisposition, boundary);
            }
            else
            {
                this.ParseUnencoded(reader, contentType, contentDisposition, boundary);
            }
            IPart part = this.CreatePart(contentType, contentDisposition);
            this.ContentWriter.Close();
            return part;
        }

        public virtual void ParseFromBase64(IMessageReader reader, ContentType contentType, ContentDisposition contentDisposition, string boundary)
        {
            byte[] buffer;
        Label_0000:
            buffer = reader.ReadLine();
            if (!reader.EndOfMessage)
            {
                switch (BoundaryChecker.CheckBoundary(buffer, boundary))
                {
                    case EBoundaryType.NotBoundary:
                    {
                        byte[] data = MailMessageRFCDecoder.DecodeFromBase64(buffer);
                        this.ContentWriter.Write(data);
                        goto Label_0000;
                    }
                    case EBoundaryType.Final:
                        this.RaiseFinalBoundaryReached();
                        break;
                }
            }
        }

        public virtual void ParseFromQuotedPrintable(IMessageReader reader, ContentType contentType, ContentDisposition contentDisposition, string boundary)
        {
            byte[] buffer;
            bool flag = false;
            byte[] data = new byte[] { 13, 10 };
        Label_0019:
            buffer = reader.ReadLine();
            if (!reader.EndOfMessage)
            {
                switch (BoundaryChecker.CheckBoundary(buffer, boundary))
                {
                    case EBoundaryType.NotBoundary:
                    {
                        byte[] buffer2;
                        if (flag)
                        {
                            this.ContentWriter.Write(data);
                        }
                        if ((buffer.Length > 0) && (buffer[buffer.Length - 1] == 0x3d))
                        {
                            buffer2 = new byte[buffer.Length - 1];
                            Array.Copy(buffer, buffer2, (int) (buffer.Length - 1));
                            flag = false;
                        }
                        else
                        {
                            buffer2 = buffer;
                            flag = true;
                        }
                        byte[] buffer4 = MailMessageRFCDecoder.DecodeFromQuotedPrintable(buffer2);
                        this.ContentWriter.Write(buffer4);
                        goto Label_0019;
                    }
                    case EBoundaryType.Final:
                        this.RaiseFinalBoundaryReached();
                        break;
                }
            }
        }

        public virtual void ParseUnencoded(IMessageReader reader, ContentType contentType, ContentDisposition contentDisposition, string boundary)
        {
            byte[] buffer;
            bool flag = false;
            byte[] data = new byte[] { 13, 10 };
        Label_0019:
            buffer = reader.ReadLine();
            if (!reader.EndOfMessage)
            {
                switch (BoundaryChecker.CheckBoundary(buffer, boundary))
                {
                    case EBoundaryType.NotBoundary:
                        if (flag)
                        {
                            this.ContentWriter.Write(data);
                        }
                        this.ContentWriter.Write(buffer);
                        flag = true;
                        goto Label_0019;

                    case EBoundaryType.Final:
                        this.RaiseFinalBoundaryReached();
                        break;
                }
            }
        }

        public abstract IContentWriter ContentWriter { get; protected set; }
    }
}

