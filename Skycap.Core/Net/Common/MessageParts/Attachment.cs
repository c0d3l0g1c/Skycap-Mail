namespace Skycap.Net.Common.MessageParts
{
    using Skycap.Net.Common;
    using Skycap.Net.Common.Exceptions;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using Windows.Storage;
    using Skycap.IO;
    using System.Threading.Tasks;
    using Windows.Storage.FileProperties;
    using Windows.UI.Xaml.Media.Imaging;

    [DataContract]
    public class Attachment
    {
        protected string _attachmentDirectory;
        protected string _contentId;
        protected Net.Common.ContentType _contentType;
        protected string _diskFilename;
        public const string _exAttachmentDirectoryCannotBeNullOrEmpty = "attachmentDirectory cannot be null or empty";
        protected string _transferFilename;
        protected ulong _size;

        public Attachment()
        {
            this._contentType = new Net.Common.ContentType("application", "octet-stream");
        }

        public Attachment(string diskFilename, string contentId, Net.Common.ContentType contentType, string attachmentDirectory, ulong size)
        {
            if (attachmentDirectory == null)
            {
                throw new ArgumentException("attachmentDirectory cannot be null or empty", "attachmentDirectory");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            if (diskFilename == null)
            {
                throw new ArgumentNullException("diskFilename");
            }
            this.TransferFilename = diskFilename;
            this._diskFilename = diskFilename;
            this._attachmentDirectory = attachmentDirectory;
            this._size = size;
            this._contentId = contentId;
            this._contentType = contentType;
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
        public string ContentID
        {
            get
            {
                return this._contentId;
            }
            set
            {
                this._contentId = value;
            }
        }

        [DataMember]
        public virtual Net.Common.ContentType ContentType
        {
            get
            {
                return this._contentType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._contentType = value;
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

        [IgnoreDataMember]
        public virtual string FullFilename
        {
            get
            {
                return Path.Combine(this._attachmentDirectory, this.DiskFilename);
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
                if (string.IsNullOrEmpty(this._transferFilename))
                    this._transferFilename = this._contentId ?? string.Empty;
                if (this._transferFilename.Contains("@"))
                    this._transferFilename = this._transferFilename.Split('@')[0];
                if (string.IsNullOrEmpty(Path.GetExtension(this._transferFilename)))
                    this._transferFilename += "." + ContentType.SubType;
                //if (this._transferFilename == Path.GetExtension(this._transferFilename))
                //    this._transferFilename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "." + ContentType.SubType;

                this._transferFilename = IOUtil.FormatFileSystemName(this._transferFilename);
            }
        }

        [IgnoreDataMember]
        public virtual string TransferFilenameExtension
        {
            get
            {
                return Path.GetExtension(TransferFilename);
            }
        }

        [IgnoreDataMember]
        public virtual string TransferFilenameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(TransferFilename);
            }
        }

        [DataMember]
        public virtual ulong Size
        {
            get
            {
                return _size;
            }
            set
            { 
                _size = value;
            }
        }

        [IgnoreDataMember]
        public virtual string SizeDisplayName
        {
            get
            {
                return IOUtil.GetMessageSize(_size);
            }
        }

        [IgnoreDataMember]
        public virtual BitmapImage Thumbnail
        {
            get
            {
                BitmapImage image = new BitmapImage();
                StorageItemThumbnail thumbnail = null;
                Task.Run(async () =>
                {
                    try
                    {
                        // If UI attachment
                        StorageFile attachmentFile = null;
                        if (IsUIAttachment)
                            attachmentFile = await StorageFile.GetFileFromPathAsync(FullFilename);
                        // Else if downloaded attachment
                        else
                        {
                            StorageFolder folder = await IOUtil.GetCreateFolder(Path.GetDirectoryName(FullFilename), FolderType.Message);
                            attachmentFile = await IOUtil.GetCreateFile(folder, Path.GetFileName(FullFilename), CreationCollisionOption.ReplaceExisting);
                        }

                        // Get the thumbnail
                        StorageFile newStorageFile = await attachmentFile.CopyAsync(ApplicationData.Current.TemporaryFolder, TransferFilename, NameCollisionOption.ReplaceExisting);
                        thumbnail = await newStorageFile.GetThumbnailAsync(ThumbnailMode.DocumentsView);
                    }
                    catch { }
                }).Wait();

                if (thumbnail != null)
                    image.SetSource(thumbnail);

                return image;
            }
        }

        [IgnoreDataMember]
        public bool IsUIAttachment
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public string ToolTip
        {
            get
            {
                return string.Format("Filename: {1}{0}Content Type: {2}", Environment.NewLine, TransferFilenameWithoutExtension, ContentType.SubType);
            }
        }
    }
}

