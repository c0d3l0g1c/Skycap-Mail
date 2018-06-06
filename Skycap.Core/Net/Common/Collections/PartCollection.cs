namespace Skycap.Net.Common.Collections
{
    using Skycap.Net.Common.MessageParts;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;

    [CollectionDataContract]
    [DebuggerDisplay("Count = {Count}")]
    public class PartCollection : IEnumerable<IPart>, IEnumerable
    {
        protected List<IPart> _collection;

        public PartCollection()
        {
            this.Init();
        }

        public PartCollection(IEnumerable<IPart> source)
        {
            this.Init();
            this.AddRange(source);
        }

        public virtual void Add(IPart part)
        {
            if (part == null)
            {
                throw new ArgumentNullException("part");
            }
            this._collection.Add(part);
        }

        public virtual void AddRange(IEnumerable<IPart> parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException("parts");
            }
            foreach (IPart part in parts)
            {
                this.Add(part);
            }
        }

        public virtual IEnumerator<IPart> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        protected virtual void Init()
        {
            this._collection = new List<IPart>();
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

        public virtual IPart this[int Index]
        {
            get
            {
                return this._collection[Index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._collection[Index] = value;
            }
        }
    }
}

