namespace Skycap.Net.Imap.Responses
{
    using System.Runtime.Serialization;
    using Skycap.Net.Imap.Collections;
    using Skycap.Net.Imap.Exceptions;

    [DataContract]
    public class MatchedName
    {
        protected NameAttributesCollection atrributes;
        protected string delimeter;
        protected const string IncorrectResponseMessage = "Incorrect Response";
        protected string name;

        public MatchedName(IMAP4Response response)
        {
            string data = response.Data;
            int index = data.IndexOf('(');
            int num2 = data.IndexOf(')');
            if (((index == -1) || (num2 == -1)) || (num2 < index))
            {
                throw new IncorrectResponseException("Incorrect Response");
            }
            string str2 = data.Substring(index + 1, (num2 - index) - 1);
            string[] attributes = new string[0];
            if (str2.Length > 0)
            {
                attributes = str2.Split(new char[] { ' ' });
            }
            this.atrributes = new NameAttributesCollection(attributes);
            data = data.Substring(num2 + 2);
            this.delimeter = GetString(data);
            this.name = GetString(data.Substring(this.delimeter.Length + 1));
            if (this.delimeter[0] == '"')
            {
                this.delimeter = this.delimeter.Substring(1, this.delimeter.Length - 2);
            }
            if (this.name[0] == '"')
            {
                this.name = this.name.Substring(1, this.name.Length - 2);
            }
        }

        private static string GetString(string data)
        {
            if (data[0] == '"')
            {
                int num = data.IndexOf('"', 1);
                return data.Substring(0, num + 1);
            }
            int index = data.IndexOf(' ');
            if (index == -1)
            {
                return data;
            }
            return data.Substring(0, index);
        }

        [DataMember]
        public NameAttributesCollection Attributes
        {
            get
            {
                return this.atrributes;
            }
        }

        [DataMember]
        public string HierarchyDelimeter
        {
            get
            {
                return this.delimeter;
            }
        }

        [DataMember]
        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

