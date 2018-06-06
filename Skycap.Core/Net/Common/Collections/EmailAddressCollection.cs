namespace Skycap.Net.Common.Collections
{
    using Skycap.Net.Common;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Linq;

    [CollectionDataContract]
    [DebuggerDisplay("Count = {Count}")]
    public class EmailAddressCollection : IEnumerable<EmailAddress>, IEnumerable
    {
        protected List<EmailAddress> _collection;

        public EmailAddressCollection()
        {
            this.Init();
        }

        public EmailAddressCollection(IEnumerable<EmailAddress> source)
        {
            this.Init();
            this.AddRange(source);
        }

        public virtual void Add(EmailAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this._collection.Add(address);
        }

        public virtual void AddRange(IEnumerable<EmailAddress> addresses)
        {
            foreach (EmailAddress address in addresses)
            {
                this.Add(address);
            }
        }

        public virtual IEnumerator<EmailAddress> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        protected virtual void Init()
        {
            this._collection = new List<EmailAddress>();
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

        public string DisplayName
        {
            get
            {
                return string.Join("; ", (from emailAddress in this._collection
                                          select emailAddress.ToString())
                                         .ToArray());
            }
        }

        public virtual EmailAddress this[int Index]
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

        public IEnumerable<string> GetEmailAddresses()
        {
            return this.Select(o => o.Address);
        }
    }
}

