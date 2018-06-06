namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Common.MessageReaders;
    using System;
    using System.Text;
    using Windows.Storage;

    public class MultiPartParser : BaseComplexPartParser
    {
        protected bool _childrenReachedFinalBoundary;
        protected readonly Encoding _defaultEncoding = new UTF8Encoding();

        protected override void childParser_FinalBoundaryReached(object sender, EventArgs e)
        {
            this._childrenReachedFinalBoundary = true;
        }

        public virtual MultiPart Parse(IMessageReader reader, EContentTransferEncoding contentTransferEncoding, string boundary, string uid, string attachmentDirectory, PopMessage parentMessage, ContentType contentType)
        {
            byte[] buffer;
            MultiPart part = new MultiPart();
            string str2 = contentType.Attributes["boundary"];
            this._childrenReachedFinalBoundary = false;
            do
            {
                buffer = reader.ReadLine();
            }
            while (buffer != null && this._defaultEncoding.GetString(buffer, 0, buffer.Length) != ("--" + str2));
            do
            {
                string source = Utils.ReadHeaders(reader);
                MIMEHeader header = new MIMEHeaderParser().Parse(source);
                if (reader.EndOfMessage)
                {
                    break;
                }
                IPart part2 = this.ParsePart(reader, header.ContentID, header.ContentType, header.ContentDisposition, header.ContentTransferEncoding, uid, attachmentDirectory, str2, parentMessage);
                part2.Header = header;
                part2.Header.ContentTransferEncoding = header.IsDefaultContentTransferEncoding ? contentTransferEncoding : header.ContentTransferEncoding;
                part.Parts.Add(part2);
            }
            while (!this._childrenReachedFinalBoundary);
            while (!reader.EndOfMessage)
            {
                buffer = reader.ReadLine();
                if (buffer == null)
                {
                    this.RaiseFinalBoundaryReached();
                    return part;
                }
                EBoundaryType type = BoundaryChecker.CheckBoundary(buffer, boundary);
                if (type != EBoundaryType.NotBoundary)
                {
                    if (type == EBoundaryType.Final)
                    {
                        this.RaiseFinalBoundaryReached();
                    }
                    return part;
                }
            }
            return part;
        }
    }
}

