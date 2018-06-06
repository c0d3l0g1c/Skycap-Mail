namespace Skycap.Net.Pop3
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerDisplay("Count = {Count}")]
    public class Pop3MessageUIDInfoCollection : IEnumerable<Pop3MessageUIDInfo>, IEnumerable
    {
        protected List<Pop3MessageUIDInfo> _collection;

        public Pop3MessageUIDInfoCollection()
        {
            this.Init();
        }

        public Pop3MessageUIDInfoCollection(IEnumerable<Pop3MessageUIDInfo> source)
        {
            this.Init();
            this.AddRange(source);
        }

        public virtual void Add(Pop3MessageUIDInfo part)
        {
            if (part == null)
            {
                throw new ArgumentNullException("part");
            }
            this._collection.Add(part);
        }

        public virtual void AddRange(IEnumerable<Pop3MessageUIDInfo> parts)
        {
            this._collection.AddRange(parts);
        }

        public virtual IEnumerator<Pop3MessageUIDInfo> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        protected virtual void Init()
        {
            this._collection = new List<Pop3MessageUIDInfo>();
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

        public virtual Pop3MessageUIDInfo this[int index]
        {
            get
            {
                return this._collection[index];
            }
            set
            {
                this._collection[index] = value;
            }
        }
    }
}

