// Type: Skycap.Net.Smtp.MailMessageRFCEncoder
// Assembly: Skycap.Net, Version=1.0.0.24824, Culture=neutral, PublicKeyToken=null
// MVID: 6076A7DE-E412-4387-8CE8-F9AF22C439CA
// Assembly location: C:\Program Files (x86)\ComponentAce\Email.NET\Bin\Email.NET.dll

using Skycap.Net.Common;
using Skycap.Net.Common.MessageParts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Skycap.Net.Smtp;
using Skycap.Net.Common.MessageParts;
using Skycap.Net.Common;
using Skycap.IO;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Skycap.Net.Smtp
{
    /// <summary>
    /// Encoder of Message class using MIME standart
    /// 
    /// </summary>
    public static class MailMessageRFCEncoder
    {
        /// <summary>
        /// Text for the exception that is thrown if firstLineLength parameter is bigger than maxLineLength
        /// 
        /// </summary>
        public const string FirstLineLengthShouldBeLessThanMaxLineLengthMessage = "firstLineLength must be less than maxLineLength";
        /// <summary>
        /// Text for a text/plain content type header
        /// 
        /// </summary>
        public const string PlainTextContentTypeHeader = "Content-Type: text/plain; charset={0}";
        /// <summary>
        /// Text for a multipart/alternative content type header
        /// 
        /// </summary>
        public const string MultipartAlternativeContentTypeHeader = "Content-Type: multipart/alternative; boundary=\"{0}\"";
        /// <summary>
        /// Text for a multipart/mixed content type header
        /// 
        /// </summary>
        public const string MultipartContentTypeHeader = "Content-Type: multipart/mixed; boundary=\"{0}\"";
        /// <summary>
        /// Text for a text/html content type header
        /// 
        /// </summary>
        public const string HtmlTextContentTypeHeader = "Content-Type: text/html; charset={0}";
        /// <summary>
        /// Text for a MIME-Version header
        /// 
        /// </summary>
        public const string MIMEHeader = "MIME-Version: 1.0";
        /// <summary>
        /// Text for an application/octet-stream content type header
        /// 
        /// </summary>
        public const string ApplicationContentTypeHeader = "Content-Type: {0}; name=\"{1}\"";
        /// <summary>
        /// Text for a Content-Disposition content type header
        /// 
        /// </summary>
        public const string ContentDispositionHeader = "Content-Disposition: attachment; filename=\"{0}\"";
        /// <summary>
        /// Text for a Content-Transfer-Encoding header
        /// 
        /// </summary>
        public const string ContentTransferEncodingBase64Header = "Content-Transfer-Encoding: base64";
        /// <summary>
        /// Maximum line length according to the MIME
        /// 
        /// </summary>
        public const int MaxMIMELineLength = 76;

        /// <summary>
        /// Returns encoded message as iterator of strings
        /// 
        /// </summary>
        /// <param name="message">Message to encode</param><param name="encoding">Encoding of text part to use</param>
        /// <returns>
        /// Iterator through encoded strings, that are ready to sent to smtp server
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Neither argument can be null</exception><exception cref="T:System.InvalidOperationException">HTML content type is specified, but PlainText field is not initialized</exception><exception cref="T:System.NotSupportedException">Only PlainText and HTML content type is supported</exception>
        public static IEnumerable<string> GetEncoded(SmtpMessage message, Encoding encoding)
    {
      // ISSUE: fault handler
        if (message == null)
          throw new ArgumentNullException("message");
        if (encoding == null)
          throw new ArgumentNullException("encoding");
        yield return "MIME-Version: 1.0";
        yield return string.Format("From:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddress(message.From, encoding, 76 - "From:".Length));
        if (message.Sender != null)
          yield return string.Format("Sender:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddress(message.Sender, encoding, 76 - "Sender:".Length));
        if (message.ReplyTo.Count > 0)
          yield return string.Format("Reply-To:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddressCollection((IEnumerable<EmailAddress>) message.ReplyTo, encoding, 76 - "Reply-To:".Length));
        if (message.To.Count > 0)
          yield return string.Format("To:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddressCollection((IEnumerable<EmailAddress>) message.To, encoding, 76 - "To:".Length));
        if (message.CarbonCopies.Count > 0)
          yield return string.Format("Cc:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddressCollection((IEnumerable<EmailAddress>) message.CarbonCopies, encoding, 76 - "Cc:".Length));
        yield return string.Format("Subject:{0}", (object) MailMessageRFCEncoder.MimeEncodeHeader(message.Subject, 76, 76 - "Subject:".Length, encoding));
        if (message.Date != DateTime.MinValue)
          yield return string.Format("Date:{0}", (object) message.Date.ToUniversalTime().ToString("r"));
        else
          yield return string.Format("Date:{0}", (object) DateTime.UtcNow.ToUniversalTime().ToString("r"));
        if (!string.IsNullOrEmpty(message.InReplyTo))
          yield return string.Format("In-Reply-To:{0}", (object) message.InReplyTo);
        if (!string.IsNullOrEmpty(message.References))
          yield return string.Format("References:{0}", (object) message.References);
        if (message.ResentDate != DateTime.MinValue)
          yield return string.Format("Resent-Date:{0}", (object) message.ResentDate.ToUniversalTime().ToString("r"));
        if (message.ResentFrom != null)
          yield return string.Format("Resent-From:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddress(message.ResentFrom, encoding, 76 - "Resent-From:".Length));
        if (message.ResentSender != null)
          yield return string.Format("Resent-Sender:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddress(message.ResentSender, encoding, 76 - "Resent-Sender:".Length));
        if (message.ResentTo != null && message.ResentTo.Count > 0)
          yield return string.Format("Resent-To:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddressCollection((IEnumerable<EmailAddress>) message.ResentTo, encoding, 76 - "Resent-To:".Length));
        if (message.ResentCarbonCopies != null && message.ResentCarbonCopies.Count > 0)
          yield return string.Format("Resent-Cc:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddressCollection((IEnumerable<EmailAddress>) message.ResentCarbonCopies, encoding, 76 - "Resent-Cc:".Length));
        if (message.ResentBlindedCarbonCopies != null && message.ResentBlindedCarbonCopies.Count > 0)
          yield return string.Format("Resent-Bcc:{0}", (object) MailMessageRFCEncoder.EncodeEmailAddressCollection((IEnumerable<EmailAddress>) message.ResentBlindedCarbonCopies, encoding, 76 - "Resent-Bcc:".Length));
        if (!string.IsNullOrEmpty(message.ResentMessageID))
          yield return string.Format("Resent-Message-ID:{0}", (object) message.ResentMessageID);
        if (!string.IsNullOrEmpty(message.Comments))
          yield return string.Format("Comments:{0}", (object) MailMessageRFCEncoder.MimeEncodeHeader(message.Comments, 76, 76 - "Comments:".Length, encoding));
        if (message.Keywords != null && message.Keywords.Count > 0)
          yield return string.Format("Keywords:{0}", (object) MailMessageRFCEncoder.MimeEncodeHeader(MailMessageRFCEncoder.StringJoin((IEnumerable<string>) message.Keywords, ", "), 76, 76 - "Keywords:".Length, encoding));
        IEnumerator<KeyValuePair<string, string>> enumerator1 = message.ExtraHeaders.GetEnumerator();
        while (enumerator1.MoveNext())
        {
          KeyValuePair<string, string> pair = enumerator1.Current;
          yield return string.Format("{0}: {1}", (object) pair.Key, (object) pair.Value);
        }
        if (enumerator1 != null)
          enumerator1.Dispose();
        if (message.Attachments.Count > 0)
        {
          Guid boundaryId = Guid.NewGuid();
          yield return string.Format("Content-Type: multipart/mixed; boundary=\"{0}\"", (object) boundaryId);
          yield return "";
          yield return string.Format("--{0}", (object) boundaryId);
          IEnumerator<string> enumerator2 = MailMessageRFCEncoder.EncodeMessageText((MailMessage) message, encoding).GetEnumerator();
          while (enumerator2.MoveNext())
          {
            string str = enumerator2.Current;
            yield return str;
          }
          if (enumerator2 != null)
            enumerator2.Dispose();
          IEnumerator<Attachment> enumerator3 = message.Attachments.GetEnumerator();
          while (enumerator3.MoveNext())
          {
            Attachment attachment = enumerator3.Current;
            yield return "";
            yield return string.Format("--{0}", (object) boundaryId);
            yield return string.Format("Content-Type: {0}; name=\"{1}\"", (object) (attachment.ContentType.Type + "/" + attachment.ContentType.SubType), (object) attachment.TransferFilename);
            yield return string.Format("Content-Disposition: attachment; filename=\"{0}\"", (object) attachment.TransferFilename);
            yield return "Content-Transfer-Encoding: base64";
            yield return "";
            IEnumerator<string> enumerator4 = MailMessageRFCEncoder.Base64EncodeAttachment(attachment).GetEnumerator();
            while (enumerator4.MoveNext())
            {
              string line = enumerator4.Current;
              yield return line;
            }
            if (enumerator4 != null)
              enumerator4.Dispose();
          }
          if (enumerator3 != null)
            enumerator3.Dispose();
          yield return "";
          yield return string.Format("--{0}--", (object) boundaryId.ToString());
        }
        else
        {
          IEnumerator<string> enumerator2 = MailMessageRFCEncoder.EncodeMessageText((MailMessage) message, encoding).GetEnumerator();
          while (enumerator2.MoveNext())
          {
            string str = enumerator2.Current;
            yield return str;
          }
          if (enumerator2 != null)
            enumerator2.Dispose();
        }
    }

        /// <summary>
        /// Encodes text part of the message.
        ///             If the specified contenttype is HTML an alternative multipart encoding will be used, for the text/html part of the message will be used the Text field, for text/plain - the PlainText field.
        ///             If the specified contenttype is PlainText, only the field Text will be used for the text/plain part.
        /// 
        /// </summary>
        /// <param name="message">A message to encode</param><param name="encoding">An encoding to use</param>
        /// <returns>
        /// Iterator for the strings of the encoded message
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">HTML content type is specified, but PlainText field is not initialized</exception><exception cref="T:System.NotSupportedException">Only PlainText and HTML content type is supported</exception>
        private static IEnumerable<string> EncodeMessageText(MailMessage message, Encoding encoding)
        {
            if (message.TextContentType == ETextContentType.Plain)
            {
                yield return string.Format("Content-Type: text/plain; charset={0}", (object)encoding.WebName.ToUpper());
                yield return "Content-Transfer-Encoding: base64";
                yield return "";
                string encodedMessage = Convert.ToBase64String(encoding.GetBytes(message.Text));
                int position = 0;
                while (position + 76 < encodedMessage.Length)
                {
                    yield return encodedMessage.Substring(position, 76);
                    position += 76;
                }
                if (position < encodedMessage.Length)
                    yield return encodedMessage.Substring(position);
            }
            else if (message.TextContentType == ETextContentType.Html)
            {
                int position = 0;
                Guid boundary = Guid.NewGuid();
                yield return string.Format("Content-Type: multipart/alternative; boundary=\"{0}\"", (object)boundary);
                yield return "";
                yield return string.Format("--{0}", (object)boundary.ToString());
                yield return string.Format("Content-Type: text/plain; charset={0}", (object)encoding.WebName.ToUpper());
                yield return "Content-Transfer-Encoding: base64";
                yield return "";
                string encodedPlainText = Convert.ToBase64String(encoding.GetBytes(message.PlainText));
                position = 0;
                while (position + 76 < encodedPlainText.Length)
                {
                    yield return encodedPlainText.Substring(position, 76);
                    position += 76;
                }
                if (position < encodedPlainText.Length)
                    yield return encodedPlainText.Substring(position);
                yield return "";
                yield return string.Format("--{0}", (object)boundary.ToString());
                yield return string.Format("Content-Type: text/html; charset={0}", (object)encoding.WebName.ToUpper());
                yield return "Content-Transfer-Encoding: base64";
                yield return "";
                string encodedMessage = Convert.ToBase64String(encoding.GetBytes(message.Text));
                position = 0;
                while (position + 76 < encodedMessage.Length)
                {
                    yield return encodedMessage.Substring(position, 76);
                    position += 76;
                }
                if (position < encodedMessage.Length)
                    yield return encodedMessage.Substring(position);
                yield return "";
                yield return string.Format("--{0}--", (object)boundary);
            }
        }

        /// <summary>
        /// Returns encoded message as iterator of strings
        /// 
        /// </summary>
        /// <param name="message">Message to encode</param>
        /// <returns>
        /// Iterator through encoded strings, that are ready to sent to smtp server
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The message argument cannot be null</exception>
        public static IEnumerable<string> GetEncoded(SmtpMessage message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            else
                return MailMessageRFCEncoder.GetEncoded(message, message.TextEncoding);
        }

        /// <summary>
        /// Encodes the collection of the email addresses according to RFC2822
        ///             for use in message headers like "To:", "CC:", etc.
        /// 
        /// </summary>
        /// <param name="collection">A collection of the email addresses to encode</param><param name="encoding">A character encoding to use</param><param name="firstLineLength">The length of the first line of the encoded string</param>
        /// <returns>
        /// The encoded string
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Neither parameter can be null</exception><exception cref="T:System.ArgumentOutOfRangeException">firstLineLength parameter cannot be negative</exception>
        private static string EncodeEmailAddressCollection(IEnumerable<EmailAddress> collection, Encoding encoding, int firstLineLength)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (firstLineLength < 0)
                throw new ArgumentOutOfRangeException("firstLineLength");
            StringBuilder stringBuilder = new StringBuilder("");
            foreach (EmailAddress address in collection)
            {
                stringBuilder.Append(MailMessageRFCEncoder.EncodeEmailAddress(address, encoding, firstLineLength));
                stringBuilder.Append(", ");
                firstLineLength = stringBuilder.Length % 76;
            }
            stringBuilder.Remove(stringBuilder.Length - ", ".Length, ", ".Length);
            return ((object)stringBuilder).ToString();
        }

        /// <summary>
        /// Encodes one email address according to RFC2822 to use in the header
        ///             fields of the message and in the collection encoding
        /// 
        /// </summary>
        /// <param name="address">An email address to encode</param><param name="encoding">&gt;A character encoding to use</param><param name="firstLineLength">The length of the first line of the encoded string</param>
        /// <returns>
        /// The encoded email address
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Neither parameter can be null</exception><exception cref="T:System.ArgumentOutOfRangeException">firstLineLength parameter cannot be negative</exception>
        private static string EncodeEmailAddress(EmailAddress address, Encoding encoding, int firstLineLength)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (firstLineLength < 0)
                throw new ArgumentOutOfRangeException("firstLineLength");
            if (address.DisplayName == null)
                return string.Format("<{0}>", (object)address.Address);
            string str1 = string.Format("<{0}>", (object)address.Address);
            string str2 = string.Format("{0}", (object)MailMessageRFCEncoder.MimeEncodeHeader(address.DisplayName, 76, firstLineLength, encoding));
            if (str1.Length + str2.Length + 1 > 76)
                return str2 + "\r\n " + str1;
            else
                return str2 + " " + str1;
        }

        /// <summary>
        /// Encodes message header contained non-ASCII characters using MIME standart
        /// 
        /// </summary>
        /// <param name="text">Header text to encode</param><param name="maxLineLength">A maximum line length in encoded header</param><param name="firstLineLength">A length of the first line in the encoded header</param><param name="encoding">Encoding to use</param>
        /// <returns>
        /// Encoded header as string
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Neither argument can be null</exception><exception cref="T:System.ArgumentOutOfRangeException">Both maxLineLength and firstLineLength cannot be negative</exception><exception cref="T:System.ArgumentException">firstLineLength cannot be greater than maxLineLength</exception>
        public static string MimeEncodeHeader(string text, int maxLineLength, int firstLineLength, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (firstLineLength < 0)
                throw new ArgumentOutOfRangeException("firstLineLength");
            if (maxLineLength <= 0)
                throw new ArgumentOutOfRangeException("maxLineLength");
            if (maxLineLength < firstLineLength)
                throw new ArgumentException("firstLineLength must be less than maxLineLength");
            string str1;
            if (MailMessageRFCEncoder.IsAsciiCharOnly(text))
            {
                str1 = text;
            }
            else
            {
                string str2 = encoding.WebName.ToUpper();
                string str3 = Convert.ToBase64String(encoding.GetBytes(text));
                int num = str2.Length + "=??B?=".Length;
                int maxBase64LineLength = (maxLineLength - num) / 4 * 4;
                if (str3.Length > firstLineLength - num)
                {
                    int firstLineLength1 = (firstLineLength - num) / 4 * 4;
                    List<string> list = MailMessageRFCEncoder.MimeBase64Encode(text, maxBase64LineLength, firstLineLength1, encoding);
                    string[] strArray = new string[list.Count];
                    for (int index = 0; index < list.Count; ++index)
                        strArray[index] = string.Format("=?{0}?{1}?{2}?=", (object)str2, (object)"B", (object)list[index]);
                    str1 = string.Join("\r\n ", strArray);
                }
                else
                    str1 = string.Format("=?{0}?{1}?{2}?=", (object)str2, (object)"B", (object)str3);
            }
            return str1;
        }

        /// <summary>
        /// Encodes a text using base64 standard, and formats result line using
        ///             given parameters of the line lengths
        /// 
        /// </summary>
        /// <param name="text">A text to encode</param><param name="maxBase64LineLength">A maximum line length</param><param name="firstLineLength">A length of the first line</param><param name="encoding">A character encoding to use</param>
        /// <returns>
        /// Base64 encoded string
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Neither argument can be null</exception><exception cref="T:System.ArgumentOutOfRangeException">Both maxBase64LineLength and firstLineLength cannot be negative</exception><exception cref="T:System.ArgumentException">firstLineLength cannot be lower than maxBase64LineLength</exception>
        public static List<string> MimeBase64Encode(string text, int maxBase64LineLength, int firstLineLength, Encoding encoding)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (encoding == null)
                throw new ArgumentNullException("encoding");
            if (maxBase64LineLength < 0)
                throw new ArgumentOutOfRangeException("maxBase64LineLength");
            if (firstLineLength < 0)
                throw new ArgumentOutOfRangeException("firstLineLength");
            if (firstLineLength > maxBase64LineLength)
                throw new ArgumentException("firstLineLength must be less than maxLineLength");
            List<string> list1 = new List<string>();
            List<byte> list2 = new List<byte>();
            int index = 0;
            int num = firstLineLength / 4 * 3;
            while (index < text.Length)
            {
                list2.Clear();
                for (; index < text.Length && list2.Count + encoding.GetBytes(text[index].ToString()).Length < num; ++index)
                    list2.AddRange((IEnumerable<byte>)encoding.GetBytes(text[index].ToString()));
                list1.Add(Convert.ToBase64String(list2.ToArray()));
                num = maxBase64LineLength / 4 * 3;
            }
            return list1;
        }

        /// <summary>
        /// Encodes an attachment using the base64 algorithm
        /// 
        /// </summary>
        /// <param name="attachment">An attachment to encode</param>
        /// <returns>
        /// Encoded attachment in the base64 strings form
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Argument cannot be null</exception><exception cref="T:System.InvalidOperationException">Specified file must exist and be readable</exception>
        private static IEnumerable<string> Base64EncodeAttachment(Attachment attachment)
    {
      // ISSUE: fault handler
        if (attachment == null)
          throw new ArgumentNullException("attachment");
        StorageFile file = null;
        Stream stream = null;
        Task.Run(async () => 
        {
            if (await IOUtil.FileExists(attachment.FullFilename) == null)
                throw new InvalidOperationException("File doesn't exists");
            file = await StorageFile.GetFileFromPathAsync(attachment.FullFilename);
            IRandomAccessStreamWithContentType randomAccessStreamWithContentType = await file.OpenReadAsync();
            stream = randomAccessStreamWithContentType.AsStreamForRead();
        }).Wait();
        int portionSize = 57;
        byte[] buffer = new byte[portionSize];
        int readed = 0;
        do
        {
          readed = stream.Read(buffer, 0, portionSize);
          string encoded = Convert.ToBase64String(buffer, 0, readed);
          yield return encoded;
        }
        while (readed == portionSize);
        if (stream != null)
          stream.Dispose();
    }

        /// <summary>
        /// Determines whether a string contains non-ASCII characters or not
        /// 
        /// </summary>
        /// <param name="text">A string to check</param>
        /// <returns>
        /// true, if the provided string contains ASCII characters only,
        ///             otherwise false
        /// 
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Argument cannot be null</exception>
        public static bool IsAsciiCharOnly(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            else
                return !new Regex("[^\0-\x007F]").IsMatch(text);
        }

        /// <summary>
        /// Joins the provided strings inserting the provided delimiter between them
        /// 
        /// </summary>
        /// <param name="strings">Strings to join</param><param name="delimiter">A delimiter to use</param>
        /// <returns>
        /// Joined string
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Neither argument can be null</exception>
        public static string StringJoin(IEnumerable<string> strings, string delimiter)
        {
            if (strings == null)
                throw new ArgumentNullException("strings");
            if (delimiter == null)
                throw new ArgumentNullException("delimiter");
            StringBuilder stringBuilder = new StringBuilder("");
            foreach (string str in strings)
            {
                stringBuilder.Append(str);
                stringBuilder.Append(delimiter);
            }
            stringBuilder.Remove(stringBuilder.Length - delimiter.Length, delimiter.Length);
            return ((object)stringBuilder).ToString();
        }
    }
}
