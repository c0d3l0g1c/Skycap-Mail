namespace Skycap.Net.Common.ContentWriters
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Skycap.IO;
    using Windows.Storage;
    using Windows.Storage.Streams;

    public class FileContentWriter : IContentWriter
    {
        protected readonly string _attachmentDirectory;
        public const string _exWriterMustBeOpened = "Writer must be opened";
        protected string _filename;
        protected Stream _fileStream;
        private uint _size;

        public FileContentWriter(string attachmentDirectory)
        {
            this._attachmentDirectory = attachmentDirectory;
            this._size = 0;
        }

        public virtual void Close()
        {
            if (this._fileStream == null)
            {
                throw new InvalidOperationException("Writer must be opened");
            }
            this._fileStream.Dispose();
        }

        protected virtual string GetRandomFilename()
        {
            return Path.GetRandomFileName();
        }

        public async virtual void Open()
        {
            Task.Run(async() =>
            {
                this._size = 0;
                this._filename = this.GetRandomFilename();
                StorageFolder folder = await IOUtil.GetCreateFolder(this._attachmentDirectory, FolderType.Message);
                StorageFile file = await folder.CreateFileAsync(this._filename, CreationCollisionOption.ReplaceExisting);
                IRandomAccessStream ras = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false);
                this._fileStream = ras.AsStream();
            }).Wait();
        }

        public virtual void Write(byte[] data)
        {
            if (this._fileStream == null)
            {
                throw new InvalidOperationException("Writer must be opened");
            }
            this._fileStream.Write(data, 0, data.Length);
            this._size += (uint) data.Length;
        }

        public virtual string Filename
        {
            get
            {
                return this._filename;
            }
        }

        public virtual uint Size
        {
            get
            {
                return this._size;
            }
        }
    }
}

