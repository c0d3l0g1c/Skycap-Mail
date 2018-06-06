namespace Skycap.Net.Imap.Scripts
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.ContentWriters;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Commands;
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Parsers;
    using Skycap.Net.Imap.Responses;
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using Windows.Storage;
    using System.IO;

    internal class ReceiveAttach : BaseFETCHCommand
    {
        private readonly Attachment _attachmentDescription;
        private readonly string _attachmentDirectory;
        private ImapMessage _message;
        protected const ulong _partSize = 0x1000L;
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public ReceiveAttach(ImapMessage message, Attachment attachmentDescription, string attachmentDirectory)
        {
            this._message = message;
            this._attachmentDescription = attachmentDescription;
            this._attachmentDirectory = attachmentDirectory;
        }

        protected override CompletionResponse Behaviour()
        {
            Exception exception = null;
            IPart partByAttachment = this._message.GetPartByAttachment(this._attachmentDescription);
            uint commandId = base._dispatcher.SendCommand(string.Format("UID FETCH {0} (BODY.PEEK[{1}] BODY.PEEK[{1}.MIME])", this._message.Uid, this._message.GetPartIndex(partByAttachment)), base.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            while (!response.IsCompletionResponse())
            {
                if (response.Name != "FETCH")
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                ulong size = base.GetSize(response);
                FileContentWriter writer = new FileContentWriter(this._attachmentDirectory);
                try
                {
                    writer.Open();
                    string str = string.Empty;
                    while (size > 0L)
                    {
                        ulong num3 = (size > 0x1000L) ? ((ulong) 0x1000L) : size;
                        byte[] bytes = base._dispatcher.GetRawData(num3);
                        byte[] data = bytes;
                        if (partByAttachment.Header.ContentTransferEncoding == EContentTransferEncoding.Base64)
                        {
                            string str2 = str + Encoding.UTF8.GetString(bytes, 0, bytes.Length).Replace("\r\n", "");
                            data = Convert.FromBase64String(str2.Substring(0, str2.Length - (str2.Length % 4)));
                            str = str2.Substring(str2.Length - (str2.Length % 4));
                        }
                        writer.Write(data);
                        size -= num3;
                    }
                }
                catch (Exception exception2)
                {
                    exception = exception2;
                    base._dispatcher.GetRawData(size);
                }
                finally
                {
                    writer.Close();
                }
                this._attachmentDescription.DiskFilename = writer.Filename;
                byte[] rawData = base._dispatcher.GetRawData();
                string s = Encoding.UTF8.GetString(rawData, 0, rawData.Length) + "\r\n";
                rawData = Encoding.UTF8.GetBytes(s);
                Match match = new Regex("{(?<size>[0-9]+)}|(?<size>NIL)").Match(s);
                if (match.Success && (match.Groups["size"].Value != "NIL"))
                {
                    size = ulong.Parse(match.Groups["size"].Value);
                    byte[] sourceArray = base._dispatcher.GetRawData(size);
                    byte[] destinationArray = new byte[rawData.Length + sourceArray.Length];
                    Array.Copy(rawData, destinationArray, rawData.Length);
                    Array.Copy(sourceArray, 0, destinationArray, rawData.Length, sourceArray.Length);
                    this._message = new MessageDescriptionParser().Parse(this._message, destinationArray, this._message.Uid, this._attachmentDirectory);
                    base._dispatcher.GetRawData();
                }
                response = base._dispatcher.GetResponse(commandId);
            }
            if (exception != null)
            {
                throw exception;
            }
            return new CompletionResponse(response.Response);
        }

        public Attachment ReceivedAttachment
        {
            get
            {
                return this._attachmentDescription;
            }
        }
    }
}

