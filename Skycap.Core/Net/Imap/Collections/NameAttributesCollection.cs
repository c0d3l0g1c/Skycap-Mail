namespace Skycap.Net.Imap.Collections
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [CollectionDataContract]
    public class NameAttributesCollection : List<string>
    {
        protected const string UnknownAttributeMessage = "Unknown attribute";

        public NameAttributesCollection(IEnumerable<string> attributes)
        {
            foreach (string str in attributes)
            {
                base.Add(str);
            }
        }
    }
}

