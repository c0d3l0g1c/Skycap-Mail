namespace Skycap.Net.Common.Collections
{
    using Skycap.Net.Common;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;

    [CollectionDataContract]
    [DebuggerDisplay("Count = {Count}")]
    public class Rfc822MessageCollection : IEnumerable<PopMessage>, IEnumerable
    {
        protected List<PopMessage> _collection;

        public Rfc822MessageCollection()
        {
            this.Init();
        }

        public Rfc822MessageCollection(IEnumerable<PopMessage> source)
        {
            this.Init();
            this.AddRange(source);
        }

        public virtual void Add(PopMessage part)
        {
            if (part == null)
            {
                throw new ArgumentNullException("part");
            }
            this._collection.Add(part);
        }

        public virtual void AddRange(IEnumerable<PopMessage> parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException("parts");
            }
            foreach (PopMessage message in parts)
            {
                this.Add(message);
            }
        }

        public virtual IEnumerator<PopMessage> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        protected virtual void Init()
        {
            this._collection = new List<PopMessage>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        public virtual int Count
        {
            get
            {
                return this._collection.Count;
            }
        }

        public virtual PopMessage this[int index]
        {
            get
            {
                return this._collection[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._collection[index] = value;
            }
        }
    }
}

