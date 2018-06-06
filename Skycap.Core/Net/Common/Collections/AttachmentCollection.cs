namespace Skycap.Net.Common.Collections
{
    using Skycap.Net.Common.MessageParts;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Linq;

    [CollectionDataContract]
    [DebuggerDisplay("Count = {Count}")]
    public class AttachmentCollection : IEnumerable<Attachment>, IEnumerable
    {
        protected List<Attachment> _attachments = new List<Attachment>();

        public void Add(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }
            this._attachments.Add(attachment);
        }

        public void AddRange(IEnumerable<Attachment> attachments)
        {
            if (attachments == null)
            {
                throw new ArgumentNullException("attachments");
            }
            foreach (Attachment attachment in attachments)
            {
                this.Add(attachment);
            }
        }

        public void Remove(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }
            if (this._attachments.FirstOrDefault(o => o.DiskFilename.Equals(attachment.DiskFilename, StringComparison.OrdinalIgnoreCase)) != null)
                this._attachments.Remove(attachment);
        }

        public bool Contains(Attachment attachment)
        {
            return this.Any(o => o.DiskFilename.Equals(attachment.DiskFilename, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerator<Attachment> GetEnumerator()
        {
            return this._attachments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._attachments.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this._attachments.Count;
            }
        }

        public Attachment this[int index]
        {
            get
            {
                return this._attachments[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._attachments[index] = value;
            }
        }

        public Attachment this[string idOrName]
        {
            get
            { 
                return (from attachment in this._attachments
                        where (attachment.ContentID == idOrName)
                           || (attachment.TransferFilename == idOrName)
                        select attachment)
                       .FirstOrDefault();
            }
        }
    }
}

