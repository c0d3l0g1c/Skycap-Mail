namespace Skycap.Net.Common.MessageParts
{
    using Skycap.Net.Common.Exceptions;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using Windows.Storage;

    [DataContract]
    [DebuggerDisplay("{ContentType}")]
    [KnownType(typeof(Attachment))]
    [KnownType(typeof(BaseContentPart))]
    [KnownType(typeof(ContentPart))]
    [KnownType(typeof(MessagePart))]
    [KnownType(typeof(MultiPart))]
    [KnownType(typeof(TextPart))]
    [KnownType(typeof(MessageDeliveryStatusPart))]
    [KnownType(typeof(MessageDispositionNotificationPart))]
    public class ContentPart : BaseContentPart
    {
        protected string _attachmentDirectory;
        protected string _diskFilename;
        public const string _exAttachmentDirectoryCannotBeNullOrEmpty = "attachmentDirectory cannot be null or empty";
        protected uint _size;
        protected string _transferFilename;

        public ContentPart()
        {
            this.AttachmentDirectory = ApplicationData.Current.LocalFolder.Path;
        }

        [DataMember]
        public virtual string AttachmentDirectory
        {
            get
            {
                return this._attachmentDirectory;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("attachmentDirectory cannot be null or empty", "value");
                }
                this._attachmentDirectory = value;
            }
        }

        [DataMember]
        public virtual string DiskFilename
        {
            get
            {
                return this._diskFilename;
            }
            set
            {
                this._diskFilename = value;
            }
        }

        [DataMember]
        public virtual uint Size
        {
            get
            {
                return this._size;
            }
            set
            {
                this._size = value;
            }
        }

        [DataMember]
        public virtual string TransferFilename
        {
            get
            {
                return this._transferFilename;
            }
            set
            {
                this._transferFilename = value;
            }
        }

        [IgnoreDataMember]
        public override EPartType Type
        {
            get
            {
                return EPartType.Content;
            }
        }
    }
}

