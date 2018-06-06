namespace Skycap.Text.Converters.Html
{
    using System;

    public class HTMLForm
    {
        private FormFieldType a;
        private string b;
        private string c;
        private int d;
        private int e;
        private int f;

        public HTMLForm(HTMLNode node)
        {
            Param param;
            this.d = 20;
            this.a = HTMLTree.FieldType(node);
            if (this.a == FormFieldType.Form)
            {
                param = node.Param.ByName("action");
            }
            else
            {
                param = node.Param.ByName("value");
            }
            if (param != null)
            {
                this.b = param.Value;
            }
            else
            {
                this.b = "";
            }
            param = node.Param.ByName("name");
            if (param != null)
            {
                this.c = param.Value;
            }
            else
            {
                this.c = "";
            }
            if (this.a == FormFieldType.TextArea)
            {
                param = node.Param.ByName("row");
                if (param != null)
                {
                    this.e = HTMLTree.FontSizeFromHTML(param.Value);
                }
                else
                {
                    this.e = 2;
                }
                param = node.Param.ByName("col");
                if (param != null)
                {
                    this.d = HTMLTree.FontSizeFromHTML(param.Value);
                }
            }
            if ((this.a == FormFieldType.Text) || (this.a == FormFieldType.Password))
            {
                param = node.Param.ByName("size");
                if (param != null)
                {
                    this.d = HTMLTree.FontSizeFromHTML(param.Value);
                }
            }
            if (((this.a == FormFieldType.Text) || (this.a == FormFieldType.Password)) || (this.a == FormFieldType.Select))
            {
                int num = node.Css.Css.IndexOf("width", CssType.ForAll, "");
                if (num != -1)
                {
                    this.f = HTMLTree.FontSizeFromHTML(node.Css.Css[num].Value);
                }
                else
                {
                    this.f = this.d * 15;
                }
            }
        }

        public int Cols
        {
            get
            {
                return this.d;
            }
            set
            {
                this.d = value;
            }
        }

        public string Name
        {
            get
            {
                return this.c;
            }
            set
            {
                this.c = value;
            }
        }

        public int Rows
        {
            get
            {
                return this.e;
            }
            set
            {
                this.e = value;
            }
        }

        public FormFieldType Type
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

        public int Width
        {
            get
            {
                return this.f;
            }
            set
            {
                this.f = value;
            }
        }
    }
}

