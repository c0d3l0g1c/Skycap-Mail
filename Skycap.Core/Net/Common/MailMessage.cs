namespace Skycap.Net.Common
{
    using Skycap.Net.Common.Collections;
    using System;
    using Windows.Storage;
    using Skycap.IO;
    using System.Runtime.Serialization;
    using Skycap.Net.Common.MessageParts;
    using System.Threading.Tasks;
    using System.Text.RegularExpressions;
    using System.IO;
    using Skycap.Net.Imap;

    [DataContract]
    public class MailMessage
    {
        public const string Filename = "Message.isc";
        public const string DefaultSubject = "Add a subject";
        public const string DefaultBody = "Add a body";
        public const string DefaultSignature = "Sent from Skycap Mail";

        protected AttachmentCollection _attachments;
        protected MIMEHeader _header;
        protected string _plainText;
        protected string _text;
        protected ETextContentType _textContentType;

        public MailMessage()
        {
            Initialise();
        }

        [IgnoreDataMember]
        private Regex ImageSourceRegex
        {
            get;
            set;
        }

        [DataMember]
        public string Uid
        {
            get;
            internal set;
        }

        [DataMember]
        public virtual AttachmentCollection Attachments
        {
            get
            {
                return this._attachments;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection BlindedCarbonCopies
        {
            get
            {
                return this._header.BlindedCarbonCopies;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._header.BlindedCarbonCopies = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection CarbonCopies
        {
            get
            {
                return this._header.CarbonCopies;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._header.CarbonCopies = value;
            }
        }

        [DataMember]
        public virtual DateTime Date
        {
            get
            {
                return this._header.Date;
            }
            set
            {
                this._header.Date = value;
            }
        }

        [IgnoreDataMember]
        public virtual string DateString
        {
            get
            {
                return Date.ToString("MMMM dd, yyyy  HH:mm tt");
            }
        }
        
        [DataMember]
        public virtual EmailAddress From
        {
            get
            {
                return this._header.From;
            }
            set
            {
                //if (value == null)
                //{
                //    throw new ArgumentNullException("value");
                //}
                this._header.From = value;
            }
        }

        [DataMember]
        public virtual MIMEHeader Header
        {
            get
            {
                return this._header;
            }
            set
            {
                this._header = value;
            }
        }

        [DataMember]
        public virtual string PlainText
        {
            get
            {
                return this._plainText;
            }
            set
            {
                this._plainText = value;
            }
        }

        [DataMember]
        public virtual string TempPlainText
        {
            get;
            set;
        }

        [DataMember]
        public virtual string Subject
        {
            get
            {
                return string.IsNullOrEmpty(this._header.Subject) ? "(No subject)" : this._header.Subject;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._header.Subject = value;
            }
        }

        [DataMember]
        public virtual string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._text = value;
            }
        }

        [DataMember]
        public virtual string TempText
        {
            get;
            set;
        }

        [DataMember]
        public virtual ETextContentType TextContentType
        {
            get
            {
                return this._textContentType;
            }
            set
            {
                this._textContentType = value;
            }
        }

        [DataMember]
        public virtual EmailAddressCollection To
        {
            get
            {
                return this._header.To;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._header.To = value;
            }
        }

        [DataMember]
        public virtual string MessagePath
        {
            get;
            internal set;
        }

        [IgnoreDataMember]
        public virtual string MailboxName
        {
            get
            {
                return IOUtil.GetMailboxNameFromPath(MessagePath);
            }
        }

        [DataMember]
        public virtual uint Size
        {
            get;
            internal set;
        }

        private void Initialise()
        {
            _attachments = new AttachmentCollection();
            _header = new MIMEHeader();
            _text = string.Empty;
            _textContentType = ETextContentType.Plain;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            Initialise();
        }

        private async Task SaveAttachments(StorageFolder folder)
        {
            for (int i = 0; i < Attachments.Count; i++)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Attachments[i].FullFilename);
                try
                {
                    await file.MoveAsync(folder, Attachments[i].DiskFilename, NameCollisionOption.ReplaceExisting);
                }
                catch { }
                Attachments[i].AttachmentDirectory = folder.Path;
            }
        }

        public async Task Save()
        {
            StorageFile file = await IOUtil.GetCreateFile(MessagePath, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, IOUtil.Serialise(this));
        }

        public async Task Save(string emailAddress, string mailboxName, string messageFolderName, bool moveAttachments)
        {
            StorageFolder folder = await IOUtil.GetCreateFolder(Path.Combine(emailAddress, mailboxName, messageFolderName), FolderType.Message);
            if (moveAttachments) await SaveAttachments(folder);
            messageFolderName = messageFolderName.Replace(MailClient.MessageFolderPrefix.ToString(), "");
            if (!Uid.Equals(messageFolderName)) Uid = messageFolderName;
            MessagePath = Path.Combine(folder.Path, Filename);
            StorageFile file = await folder.CreateFileAsync(Filename, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
            await FileIO.WriteTextAsync(file, IOUtil.Serialise(this));
        }

        public MailMessage Clone()
        {
            return (MailMessage)MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            MailMessage mailMessage = (MailMessage)obj;
            return this.MailboxName.Equals(mailMessage.MailboxName, StringComparison.OrdinalIgnoreCase)
                && this.Uid.Equals(mailMessage.Uid, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.MailboxName.GetHashCode() ^ this.Uid.GetHashCode();
        }
    }
}

