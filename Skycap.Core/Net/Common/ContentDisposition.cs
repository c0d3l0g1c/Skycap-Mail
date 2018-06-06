namespace Skycap.Net.Common
{
    using Skycap.Net.Common.Collections;
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [DataContract]
    [DebuggerDisplay("{Disposition}")]
    public class ContentDisposition
    {
        protected AttributesDictionary _attributes;
        protected string _disposition;

        public ContentDisposition()
        {
            this.Attributes = new AttributesDictionary();
        }

        [IgnoreDataMember]
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
        public virtual string Disposition
        {
            get
            {
                return this._disposition;
            }
            set
            {
                this._disposition = value;
            }
        }
    }
}

