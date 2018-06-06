// Type: CuteEditor.Convertor.HTMLNode
// Assembly: CuteEditor, Version=6.6.0.0, Culture=neutral, PublicKeyToken=3858aa6802b1223a
// Assembly location: C:\Users\Public\Downloads\CuteEditor_for_NET6\Framework 2.0\bin\CuteEditor.dll

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Skycap.Text.Converters.Html
{
    public class HTMLNode
    {
        private string d = "";
        private string e = "";
        private string h = "";
        private string i = "";
        private bool j = true;
        private HTMLNode a;
        private HTMLNodes b;
        private Params c;
        private bool f;
        private Csss g;

        public HTMLNodes Nodes
        {
            get
            {
                return this.b;
            }
        }

        public Params Param
        {
            get
            {
                return this.c;
            }
        }

        public bool HasText
        {
            get
            {
                return this.j;
            }
        }

        public string TagName
        {
            get
            {
                return this.d;
            }
            set
            {
                this.d = value.Trim();
                this.db();
            }
        }

        public string Text
        {
            get
            {
                return this.e;
            }
            set
            {
                this.e = HTMLTree.GetCleanText(value);
                if (!this.d.Equals("style"))
                    return;
                this.da(this.e.Trim());
                this.e = "";
            }
        }

        public HTMLNode Parent
        {
            get
            {
                return this.a;
            }
            set
            {
                this.a = value;
                this.ParseCss();
            }
        }

        public bool IsComplete
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

        public Csss Css
        {
            get
            {
                return this.g;
            }
        }

        public string ClassName
        {
            get
            {
                return this.h;
            }
        }

        public string ID
        {
            get
            {
                return this.i;
            }
        }

        public int Width
        {
            get
            {
                int num1 = -1;
                int num2 = 0;
                int index1 = this.g.Css.IndexOf("width", CssType.ForAll, "");
                if (index1 != -1)
                {
                    if (!this.g.Css[index1].Value.EndsWith("%"))
                        num1 = HTMLTree.FontSizeFromHTML(this.g.Css[index1].Value);
                }
                else
                {
                    Param obj = this.c.ByName("width");
                    if (obj != null && !obj.Value.EndsWith("%"))
                        num1 = HTMLTree.FontSizeFromHTML(obj.Value);
                }
                if (this.TagName.Equals("div"))
                {
                    DivPos div = new DivPos();
                    HTMLTree.ParseDivStyle(this, div);
                    if (div.PosType == PositionType.Absolute || div.PosType == PositionType.Relative)
                        num2 = div.Left;
                }
                if (this.TagName.Equals("table"))
                {
                    int num3 = this.CalcTableWidth(this.b[0]);
                    if (num3 > num1)
                        num1 = num3;
                }
                else
                {
                    for (int index2 = 0; index2 < this.b.Count; ++index2)
                    {
                        int width = this.b[index2].Width;
                        if (width > num1)
                            num1 = width;
                    }
                }
                return num2 + num1;
            }
        }

        public HTMLNode()
            : this("")
        {
        }

        public HTMLNode(string tagName)
        {
            this.b = new HTMLNodes();
            this.c = new Params();
            this.g = new Csss();
            this.d = tagName.Trim();
            this.db();
            this.f = false;
        }

        private void db()
        {
            if (this.d.Equals("tr") || this.d.Equals("table") || (this.d.Equals("thead") || this.d.Equals("tbody")) || (this.d.Equals("hr") || this.d.Equals("img") || this.d.Equals(string.Empty)))
                this.j = false;
            else
                this.j = true;
        }

        public void Add(HTMLNode node)
        {
            this.b.Add(node);
            node.Parent = this;
        }

        public int CalcTableWidth(HTMLNode node)
        {
            HTMLNode htmlNode;
            for (htmlNode = node; htmlNode.TagName != "tr"; htmlNode = htmlNode.Nodes[0])
            {
                if (htmlNode.Nodes.Count == 0)
                    return -1;
            }
            int num = 0;
            for (int index = 0; index < htmlNode.Nodes.Count; ++index)
            {
                int width = htmlNode.Nodes[index].Width;
                if (width == -1)
                    return -1;
                num += width;
            }
            if (num > 0)
                return num;
            else
                return -1;
        }

        public async Task ParseCss(Params param)
        {
            if (this.g == null)
                this.g = new Csss();
            else if (this.d.Equals("link"))
            {
                Param obj = this.Param.ByName("href");
                string url = "";
                if (obj != null)
                    url = obj.Value.Trim();
                if (!(url != ""))
                    return;
                try
                {
                    this.da(new StreamReader(await NetHelper.Get(url)).ReadToEnd());
                    this.d = "style";
                }
                catch
                {
                }
            }
            else
            {
                for (int index = 0; index < param.Count; ++index)
                {
                    if (param[index].Name.ToLower().Equals("style"))
                        this.ParseStyleString(param[index].Value.ToLower(), CssType.ForAll, "");
                    else if (HTMLTree.IsCss2(param[index].Name))
                        this.g.SetNameValue(param[index].Name.ToLower(), param[index].Value.ToLower(), CssType.ForAll, "");
                }
            }
        }

        public void ParseCss()
        {
            this.ParseCss(this.c);
        }

        public void ParseStyleString(string style, CssType type, string forstr)
        {
            string str1 = style;
            char[] chArray = new char[1]
      {
        ';'
      };
            foreach (string str2 in str1.Split(chArray))
            {
                string str3 = str2.Replace("\n", "").Trim();
                int length = str3.IndexOf(":");
                if (length != -1)
                {
                    string str4 = str3.Substring(0, length).Trim();
                    if (HTMLTree.IsCss2(str4))
                        this.g.SetNameValue(str4, str3.Substring(length + 1).Trim(), type, forstr);
                }
            }
        }

        public void TransferCssBlock(HTMLNode stylenode, List<object> styletree, HTMLNode body, bool inbody)
        {
            List<object> arrayList = new List<object>();
            this.da();
            if (stylenode != null)
            {
                arrayList.Add((object)stylenode);
                for (int index = 0; index < stylenode.Css.Css.Count; ++index)
                {
                    Css css = stylenode.Css.Css[index];
                    if ((css.Type == CssType.ForClass && css.For.ToLower().Equals(this.h) || css.Type == CssType.ForTag && (css.For.Equals(this.d) || css.For.IndexOf("," + this.d + ",") >= 0)) && this.Css.Css.IndexOf(css.Name, CssType.ForAll, "") < 0)
                    {
                        css.For = "";
                        css.Type = CssType.ForAll;
                        this.Css.Css.Add(css);
                    }
                }
            }
            for (int index1 = 0; index1 < this.b.Count; ++index1)
            {
                if (this.b[index1].d.Equals("style") && styletree.IndexOf((object)this.b[index1]) < 0)
                {
                    styletree.Add((object)this.b[index1]);
                    if (inbody || body == null)
                        arrayList.Add((object)this.b[index1]);
                    else
                        body.TransferCssBlock(this.b[index1], styletree, body, true);
                }
                else
                {
                    if (this.d.Equals("body"))
                        inbody = true;
                    if (arrayList.Count > 0)
                    {
                        for (int index2 = arrayList.Count - 1; index2 >= 0; --index2)
                        {
                            HTMLNode stylenode1 = (HTMLNode)arrayList[index2];
                            this.b[index1].TransferCssBlock(stylenode1, styletree, body, inbody);
                        }
                    }
                    else
                        this.b[index1].TransferCssBlock((HTMLNode)null, styletree, body, inbody);
                }
            }
        }

        private void da(string A_0)
        {
            string[] strArray = A_0.Split(new char[1]
      {
        '}'
      });
            for (int index = 0; index < strArray.Length - 1; ++index)
            {
                string text = strArray[index].Substring(0, strArray[index].IndexOf('{')).ToLower();
                string style = strArray[index].Substring(strArray[index].IndexOf('{') + 1);
                string forstr1 = HTMLTree.GetCleanText(text);
                if (forstr1.IndexOf('.') > 0)
                {
                    string forstr2 = forstr1.IndexOf('.') != 1 ? forstr1.Substring(forstr1.IndexOf(".") + 1) : (forstr1.IndexOf(" ") <= 0 ? forstr1.Substring(1) : forstr1.Substring(1, forstr1.IndexOf(" ") - 1));
                    this.ParseStyleString(style, CssType.ForClass, forstr2);
                }
                else
                {
                    if (forstr1.IndexOf(",") > 0)
                        forstr1 = "," + forstr1 + ",";
                    this.ParseStyleString(style, CssType.ForTag, forstr1);
                }
            }
        }

        private void da()
        {
            Param obj1 = this.Param.ByName("class");
            if (obj1 != null)
                this.h = obj1.Value.Trim().ToLower();
            Param obj2 = this.Param.ByName("id");
            if (obj2 == null)
                return;
            this.i = obj2.Value.Trim().ToLower();
        }
    }
}
