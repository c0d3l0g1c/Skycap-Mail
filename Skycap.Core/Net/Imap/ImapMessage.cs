namespace Skycap.Net.Imap
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.MessageParts;
    using Skycap.Net.Imap.Collections;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.IO;

    [DataContract]
    [KnownType(typeof(PopMessage))]
    public class ImapMessage : StructuredMessage
    {
        protected Dictionary<Attachment, IPart> _attachmentToPart;
        protected MessageFlagCollection _flags;
        protected Dictionary<string, IPart> _indexToPart;
        protected Dictionary<IPart, string> _partToIndex;

        public ImapMessage()
        {
            base.Uid = "0";
            this._flags = new MessageFlagCollection();
            this._partToIndex = new Dictionary<IPart, string>();
            this._indexToPart = new Dictionary<string, IPart>();
            this._attachmentToPart = new Dictionary<Attachment, IPart>();
        }

        //public ImapMessage(uint uid, uint size, MessageFlagCollection flags, IPart rootPart)
        //{
        //    base.Uid = uid.ToString();
        //    this._size = size;
        //    this._flags = flags;
        //    base._rootPart = rootPart;
        //    this._partToIndex = new Dictionary<IPart, string>();
        //    this._indexToPart = new Dictionary<string, IPart>();
        //    this._attachmentToPart = new Dictionary<Attachment, IPart>();
        //}

        private void AddAttachment(IPart part)
        {
            ContentPart part2 = (ContentPart) part;
            Attachment attachment = new Attachment();
            attachment.DiskFilename = Path.GetRandomFileName();
            attachment.TransferFilename = part2.DiskFilename;
            attachment.ContentID = part2.Header.ContentID;
            if (!string.IsNullOrEmpty(attachment.ContentID))
                attachment.ContentID = attachment.ContentID.Replace("<", "").Replace(">", "");
            attachment.ContentType = part2.Header.ContentType;
            attachment.AttachmentDirectory = part2.AttachmentDirectory;
            attachment.Size = part2.Size;
            if (!attachment.TransferFilenameExtension.Equals(".octet-stream", StringComparison.OrdinalIgnoreCase)) this.Attachments.Add(attachment);
            this._attachmentToPart.Add(attachment, part);
        }

        public IPart FindPart(string partIndex)
        {
            return this._indexToPart[partIndex];
        }

        private string GetHtmlText(IPart node)
        {
            if (node.Type == EPartType.Multi)
            {
                MultiPart part = (MultiPart) node;
                if ((part.Header.ContentType.SubType.Equals("mixed", StringComparison.OrdinalIgnoreCase) || part.Header.ContentType.SubType.Equals("alternative", StringComparison.OrdinalIgnoreCase)) || part.Header.ContentType.SubType.Equals("related", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IPart part2 in ((MultiPart) node).Parts)
                    {
                        string htmlText = this.GetHtmlText(part2);
                        if (htmlText != null)
                        {
                            return htmlText;
                        }
                    }
                }
            }
            else if (PartUtils.IsTextPart(node.Header.ContentType, node.Header.ContentDisposition) && node.Header.ContentType.SubType.Equals("html", StringComparison.CurrentCultureIgnoreCase))
            {
                return ((TextPart) node).Text;
            }
            return null;
        }

        public IPart GetPartByAttachment(Attachment attachment)
        {
            return this._attachmentToPart[attachment];
        }

        public string GetPartIndex(IPart part)
        {
            return this._partToIndex[part];
        }

        private string GetPlainText(IPart node)
        {
            if (node.Type == EPartType.Multi)
            {
                MultiPart part = (MultiPart) node;
                if ((part.Header.ContentType.SubType.Equals("mixed", StringComparison.OrdinalIgnoreCase) || part.Header.ContentType.SubType.Equals("alternative", StringComparison.OrdinalIgnoreCase)) || part.Header.ContentType.SubType.Equals("related", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IPart part2 in ((MultiPart) node).Parts)
                    {
                        string plainText = this.GetPlainText(part2);
                        if (plainText != null)
                        {
                            return plainText;
                        }
                    }
                }
            }
            else if (PartUtils.IsTextPart(node.Header.ContentType, node.Header.ContentDisposition) && node.Header.ContentType.SubType.Equals("plain", StringComparison.CurrentCultureIgnoreCase))
            {
                return ((TextPart) node).Text;
            }
            return null;
        }

        public void InitializeInternalStructure()
        {
            this._partToIndex.Clear();
            this._indexToPart.Clear();
            this._attachmentToPart.Clear();
            if (this.RootPart.Type != EPartType.Multi)
            {
                this.SetInternaStructureForPart(this.RootPart, "1");
            }
            else
            {
                MultiPart rootPart = (MultiPart) this.RootPart;
                for (int i = 0; i < rootPart.Parts.Count; i++)
                {
                    this.SetInternaStructureForPart(rootPart.Parts[i], (i + 1).ToString());
                }
            }
        }

        private void SetInternaStructureForPart(IPart rootPart, string index)
        {
            this._partToIndex.Add(rootPart, index);
            this._indexToPart.Add(index, rootPart);
            if (rootPart.Type == EPartType.Content)
            {
                this.AddAttachment(rootPart);
            }
            if (rootPart.Type == EPartType.Multi)
            {
                MultiPart part = (MultiPart) rootPart;
                for (int i = 0; i < part.Parts.Count; i++)
                {
                    this.SetInternaStructureForPart(part.Parts[i], string.Format("{0}.{1}", index, i + 1));
                }
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            base.Uid = "0";
            this._flags = new MessageFlagCollection();
            this._partToIndex = new Dictionary<IPart, string>();
            this._indexToPart = new Dictionary<string, IPart>();
            this._attachmentToPart = new Dictionary<Attachment, IPart>();
        }

        [DataMember]
        public virtual MessageFlagCollection Flags
        {
            get
            {
                return this._flags;
            }
            protected set
            {
                this._flags = value;
            }
        }

        [IgnoreDataMember]
        public virtual bool IsAnswered
        {
            get
            {
                return this._flags.Contains(EFlag.Answered);
            }
        }

        [IgnoreDataMember]
        public virtual bool IsDraft
        {
            get
            {
                return this._flags.Contains(EFlag.Draft);
            }
        }

        [IgnoreDataMember]
        public override bool IsFlagged
        {
            get
            {
                return this._flags.Contains(EFlag.Flagged);
            }
            internal set
            {
                SetFlags(value, EFlag.Flagged);
            }
        }

        [IgnoreDataMember]
        public virtual bool IsRecent
        {
            get
            {
                return this._flags.Contains(EFlag.Recent);
            }
        }

        [IgnoreDataMember]
        public override bool IsSeen
        {
            get
            {
                return this._flags.Contains(EFlag.Seen);
            }
            internal set
            {
                SetFlags(value, EFlag.Seen);
            }
        }

        [IgnoreDataMember]
        public override bool IsDeleted
        {
            get
            {
                return this._flags.Contains(EFlag.Deleted);
            }
            internal set
            {
                SetFlags(value, EFlag.Deleted);
            }
        }

        private void SetFlags(bool @value, EFlag flag)
        {
            if (@value)
            {
                if (!this._flags.Exists(o => o.Type == flag))
                    this._flags.Add(new MessageFlag(flag));
            }
            else
            {
                if (this._flags.Exists(o => o.Type == flag))
                {
                    MessageFlag messageFlag = this._flags.Find(o => o.Type == flag);
                    this._flags.Remove(messageFlag);
                }
            }
        }
    }
}

