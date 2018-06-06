namespace Skycap.Net.Common.ContentWriters
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class StringContentWriter : IContentWriter
    {
        protected readonly Encoding _encoding;
        protected List<byte> _savedBytes;
        protected StringBuilder _text;

        public StringContentWriter(string charset)
        {
            if (charset == null)
            {
                throw new ArgumentNullException("charset");
            }
            this._savedBytes = new List<byte>();
            try
            {
                if (charset == "us-ascii")
                {
                    this._encoding = Encoding.GetEncoding("Latin1");
                }
                else
                {
                    this._encoding = Encoding.GetEncoding(charset);
                }
            }
            catch (ArgumentException)
            {
                this._encoding = new UTF8Encoding();
            }
        }

        public virtual void Close()
        {
        }

        public virtual void Open()
        {
            this._text = new StringBuilder();
        }

        public virtual void Write(byte[] data)
        {
            this._savedBytes.AddRange(data);
        }

        public virtual string Text
        {
            get
            {
                return this._encoding.GetString(this._savedBytes.ToArray(), 0, this._savedBytes.Count);
            }
        }
    }
}

