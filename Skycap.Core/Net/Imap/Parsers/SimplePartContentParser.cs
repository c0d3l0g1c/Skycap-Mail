namespace Skycap.Net.Imap.Parsers
{
    using Skycap.Net.Common;
    using System;
    using System.Text;

    internal class SimplePartContentParser
    {
        public string Parse(byte[] source, EContentTransferEncoding contentTransferEncoding, string charset)
        {
            Encoding encoding;
            switch (contentTransferEncoding)
            {
                case EContentTransferEncoding.Base64:
                    return MailMessageRFCDecoder.GetStringFromBase64(source, charset);

                case EContentTransferEncoding.QuotedPrintable:
                    return MailMessageRFCDecoder.GetStringFromQuotedPrintable(source, charset);
            }
            try
            {
                encoding = Encoding.GetEncoding(charset);
            }
            catch (ArgumentException)
            {
                return Encoding.UTF8.GetString(source, 0, source.Length);
            }
            return encoding.GetString(source, 0, source.Length);
        }
    }
}

