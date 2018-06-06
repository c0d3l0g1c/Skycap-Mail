namespace Skycap.Net.Common.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [CollectionDataContract]
    [DebuggerDisplay("Count = {Count}")]
    public class ExtraHeadersDictionary : IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        private Dictionary<string, string> _innerDictionary = new Dictionary<string, string>();

        public void Add(KeyValuePair<string, string> item)
        {
            this._innerDictionary.Add(item.Key, item.Value);
        }

        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this._innerDictionary.Add(key, value);
        }

        public void Clear()
        {
            this._innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return (this._innerDictionary.ContainsKey(item.Key) && (item.Value == this._innerDictionary[item.Key]));
        }

        public bool ContainsKey(string key)
        {
            return this._innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < array.Length; i++)
            {
                this.Add(array[i].Key, array[i].Value);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this._innerDictionary.GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return this._innerDictionary.Remove(item.Key);
        }

        public bool Remove(string key)
        {
            return this._innerDictionary.Remove(key);
        }

        public virtual bool SmartAdd(string key, string value)
        {
            return DictionaryExtenders.SmartAdd(this, key, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            return this._innerDictionary.TryGetValue(key, out value);
        }

        [IgnoreDataMember]
        public int Count
        {
            get
            {
                return this._innerDictionary.Count;
            }
        }

        [IgnoreDataMember]
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        [DataMember]
        public string this[string key]
        {
            get
            {
                return this._innerDictionary[key];
            }
            set
            {
                this._innerDictionary[key] = value;
            }
        }

        [IgnoreDataMember]
        public ICollection<string> Keys
        {
            get
            {
                return this._innerDictionary.Keys;
            }
        }

        [IgnoreDataMember]
        public ICollection<string> Values
        {
            get
            {
                return this._innerDictionary.Values;
            }
        }
    }
}

