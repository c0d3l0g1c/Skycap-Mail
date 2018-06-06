namespace Skycap.Net.Common
{
    using Skycap.Net.Common.Collections;
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Runtime.Serialization;

    [DataContract]
    [DebuggerDisplay("{Type}/{SubType}")]
    public class ContentType : IComparable
    {
        protected AttributesDictionary _attributes;
        protected string _subType;
        protected string _type;

        public ContentType() : this("text", "plain")
        {
            this.Attributes = new AttributesDictionary();
        }

        public ContentType(string type, string subType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (subType == null)
            {
                throw new ArgumentNullException("subType");
            }
            if (!this.IsValidType(type))
            {
                throw new ArgumentException("type");
            }
            if (!this.IsValidType(subType))
            {
                throw new ArgumentException("subtype");
            }
            this.Type = type;
            this.SubType = subType;
            this.Attributes = new AttributesDictionary();
        }

        public virtual int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            ContentType type = obj as ContentType;
            if (type == null)
            {
                throw new InvalidCastException("obj");
            }
            int num = string.Compare(this.Type, type.Type, StringComparison.CurrentCultureIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            return string.Compare(this.SubType, type.SubType, StringComparison.CurrentCultureIgnoreCase);
        }

        protected virtual bool IsValidType(string type)
        {
            Regex regex = new Regex("[\x0001-\x007f]");
            return regex.IsMatch(type);
        }

        public override string ToString()
        {
            return string.Format("Content-Type: {0}/{1}", this.Type, this.SubType);
        }

        [DataMember]
        public virtual AttributesDictionary Attributes
        {
            get
            {
                return this._attributes;
            }
            protected set
            {
                this._attributes = value;
            }
        }

        [DataMember]
        public virtual string SubType
        {
            get
            {
                return this._subType;
            }
            protected set
            {
                this._subType = value;
            }
        }

        [DataMember]
        public virtual string Type
        {
            get
            {
                return this._type;
            }
            protected set
            {
                this._type = value;
            }
        }
    }
}

