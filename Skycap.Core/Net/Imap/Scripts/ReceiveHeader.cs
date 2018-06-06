namespace Skycap.Net.Imap.Scripts
{
    using Skycap.Net.Imap;
    using Skycap.Net.Imap.Collections;
    using Skycap.Net.Imap.Commands;
    using Skycap.Net.Imap.Events;
    using Skycap.Net.Imap.Exceptions;
    using Skycap.Net.Imap.Parsers;
    using Skycap.Net.Imap.Responses;
    using Skycap.Net.Imap.Sequences;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Windows.Storage;
    using Skycap.Net.Common.Exceptions;

    internal class ReceiveHeader : BaseFETCHCommand
    {
        private readonly string _attachmentDirectory;
        protected Skycap.Net.Imap.Collections.MessageCollection _fetchedItems;
        protected SequenceSet _sequence = new SequenceSet();
        protected bool _uidMode;

        public event EventHandler<BrokenMessageInfoArgs> BrokenMessage;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public ReceiveHeader(ISequence range, bool uidMode, string attachmentDirectory)
        {
            this._sequence.Add(range);
            this._uidMode = uidMode;
            this._attachmentDirectory = attachmentDirectory;
            this._fetchedItems = new Skycap.Net.Imap.Collections.MessageCollection();
        }

        protected override CompletionResponse Behaviour()
        {
            uint commandId = base._dispatcher.SendCommand(string.Format("{0}FETCH {1} (FLAGS UID RFC822.SIZE INTERNALDATE BODY.PEEK[HEADER] BODY)", this._uidMode ? "UID " : "", this._sequence), base.filter);
            IMAP4Response response = base._dispatcher.GetResponse(commandId);
            if (!response.IsCompletionResponse())
            {
                if (response.Name != "FETCH")
                {
                    throw new UnexpectedResponseException("Unexpected response");
                }
                while (!response.IsCompletionResponse())
                {
                    try
                    {
                        List<byte> list = new List<byte>();
                        if (!response.IsCompletionResponse())
                        {
                            ulong size = base.GetSize(response);
                            list.AddRange(Encoding.UTF8.GetBytes(response.Data + "\r\n"));
                            list.AddRange(base._dispatcher.GetRawData(size));
                            list.AddRange(base._dispatcher.GetRawData());
                            int sourceIndex = -1;
                            int num4 = -1;
                            byte num5 = Encoding.UTF8.GetBytes("(")[0];
                            byte num6 = Encoding.UTF8.GetBytes(")")[0];
                            for (int i = 0; i < list.Count; i++)
                            {
                                if ((sourceIndex == -1) && (list[i] == num5))
                                {
                                    sourceIndex = i;
                                }
                                if (list[i] == num6)
                                {
                                    num4 = i;
                                }
                            }
                            sourceIndex++;
                            byte[] destinationArray = new byte[num4 - sourceIndex];
                            Array.Copy(list.ToArray(), sourceIndex, destinationArray, 0, num4 - sourceIndex);
                            ImapMessage item = new MessageDescriptionParser().Parse(destinationArray, _sequence[0].ToString(), this._attachmentDirectory);
                            this._fetchedItems.Add(item);
                            if (this.MessageReceived != null)
                            {
                                this.MessageReceived(this, new MessageReceivedEventArgs(item));
                            }
                        }
                    }
                    catch (BadAttachmentDirectoryException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        if (this.BrokenMessage != null)
                        {
                            this.BrokenMessage(this, new BrokenMessageInfoArgs(exception.Message));
                        }
                    }
                    response = base._dispatcher.GetResponse(commandId);
                }
            }
            return new CompletionResponse(response.Response);
        }

        public Skycap.Net.Imap.Collections.MessageCollection MessageCollection
        {
            get
            {
                return this._fetchedItems;
            }
        }
    }
}

