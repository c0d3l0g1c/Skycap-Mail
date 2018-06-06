namespace Skycap.Text.Converters.Html
{
    using System;

    public class Param
    {
        private string a;
        private string b;

        public Param()
        {
            this.a = "";
            this.b = "";
            this.a = "";
            this.b = "";
        }

        public Param(string name, string value)
        {
            this.a = "";
            this.b = "";
            this.a = name;
            this.b = value;
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", this.a, this.b);
        }

        public string Name
        {
            get
            {
                return this.a;
            }
            set
            {
                this.a = value;
            }
        }

        public string Value
        {
            get
            {
                return this.b;
            }
            set
            {
                this.b = value;
            }
        }
    }
}

