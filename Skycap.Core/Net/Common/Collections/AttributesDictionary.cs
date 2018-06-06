namespace Skycap.Net.Common.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Serialization;

    [CollectionDataContract]
    [DebuggerDisplay("Count = {Count}")]
    public class AttributesDictionary : Dictionary<string, string>
    {
        public new virtual void Add(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            base.Add(key.ToLower(), value);
        }

        public new virtual bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return base.ContainsKey(key.ToLower());
        }

        public virtual bool SmartAdd(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return DictionaryExtenders.SmartAdd(this, key.ToLower(), value);
        }

        public new virtual string this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                string value = string.Empty;
                TryGetValue(key.ToLower(), out value);
                return value;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                base[key.ToLower()] = value;
            }
        }
    }
}

