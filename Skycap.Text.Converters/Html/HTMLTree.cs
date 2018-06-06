namespace Skycap.Text.Converters.Html
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Skycap.Text.Converters.Html;
    using Windows.UI;

    public class HTMLTree
    {
        private const string a = ":align:azimuth:background:background-attachment:background-color:background-image:background-position:background-repeat:border:border-collapse:border-color:border-spacing:border-style:border-top:border-right:border-bottom:border-left:border-top-color:border-right-color:border-bottom-color:border-left-color:border-top-style:border-right-style:border-bottom-style:border-left-style:border-top-width:border-right-width:border-bottom-width:border-left-width:border-width:bottom:caption-side:clear:clip:color:content:counter-increment:counter-reset:cue:cue-after:cue-before:cursor:direction:display:elevation:empty-cells:float:font:font-family:font-size:font-size-adjust:font-stretch:font-style:font-variant:font-weight:height:left:letter-spacing:line-height:list-style:list-style-image:list-style-position:list-style-type:margin:margin-top:margin-right:margin-bottom:margin-left:marker-offset:marks:max-height:max-width:min-height:min-width:orphans:outline:outline-color:outline-style:outline-width:overflow:padding:padding-top:padding-right:padding-bottom:padding-left:page:page-break-after:page-break-before:page-break-inside:pause:pause-after:pause-before:pitch:pitch-range:play-during:position:quotes:richness:right:size:speak:speak-header:speak-numeral:speak-punctuation:speech-rate:stress:table-layout:text-align:text-decoration:text-indent:text-shadow:text-transform:top:unicode-bidi:vertical-align:visibility:voice-family:volume:white-space:widows:width:word-spacing:z-index:";
        private string b = "";
        private HTMLNode c;
        private HTMLNode d;
        public static Color defaultColor = Colors.Black;
        public static string defaultFontName = "Times New Roman";
        public static uint defaultFontSize = 0x18;
        public const int DefaultMargin = 3;
        private HTMLNode e;
        private HTMLNode f;

        public HTMLTree(string html)
        {
            this.b = new Regex(@"<style[^>]*>\s*<!--", RegexOptions.Singleline | RegexOptions.IgnoreCase).Replace(html, "<style>");
            this.b = new Regex(@"-->\s*</style>", RegexOptions.Singleline | RegexOptions.IgnoreCase).Replace(this.b, "</style>");
            this.b = new Regex("<!--.*?-->", RegexOptions.Singleline).Replace(this.b, "");
            this.c = new HTMLNode();
            this.da();
            this.da(this.c);
            HTMLNode stylenode = null;
            List<object> styletree = new List<object>();
            this.c.TransferCssBlock(stylenode, styletree, this.BodyNode, false);
        }

        private void da()
        {
            int startIndex = 0;
            string str = "";
            int index = 0;
            string str2 = "";
            bool flag = false;
            HTMLNode node = null;
            HTMLNode node2 = null;
            bool flag2 = false;
            startIndex = this.b.IndexOf("<", startIndex);
            if (startIndex > 0)
            {
                node = new HTMLNode("font") {
                    Text = this.b.Substring(0, startIndex)
                };
                this.c.Nodes.Add(node);
                node.IsComplete = true;
            }
            else
            {
                startIndex = 0;
            }
            while (startIndex < this.b.Length)
            {
                string str3;
                flag2 = false;
                index = this.b.IndexOf("<", startIndex);
                if (index == -1)
                {
                    break;
                }
                str2 = this.b.Substring(index, (this.b.IndexOf(">", index) - index) + 1);
                startIndex = index + str2.Length;
                if (str2.StartsWith("</"))
                {
                    flag = true;
                    str2 = str2.Substring(2);
                }
                else
                {
                    flag = false;
                    str2 = str2.Substring(1);
                    node = new HTMLNode();
                }
                if (str2.EndsWith("/>"))
                {
                    flag2 = true;
                    str2 = str2.Substring(0, str2.Length - 2);
                }
                else
                {
                    flag2 = false;
                    str2 = str2.Substring(0, str2.Length - 1);
                }
                if (str2.IndexOf(" ") > 0)
                {
                    str = str2.Substring(0, str2.IndexOf(" "));
                }
                else
                {
                    str = str2;
                }
                str = str.ToLower();
                if (flag)
                {
                    HTMLNode parent = null;
                    parent = node2;
                    while (parent != null)
                    {
                        if (parent.TagName.Equals(str) && !parent.IsComplete)
                        {
                            break;
                        }
                        parent = parent.Parent;
                    }
                    if (parent != null)
                    {
                        parent.IsComplete = true;
                        node2 = parent.Parent;
                        node = parent;
                    }
                }
                else
                {
                    node.TagName = str;
                    if (str2.IndexOf(" ") != -1)
                    {
                        this.db(node, str2);
                    }
                    if (node2 == null)
                    {
                        this.c.Add(node);
                    }
                    else
                    {
                        string str5;
                        if (((str5 = node2.TagName) != null) && (((str5 == "li") || (str5 == "option")) || ((str5 == "dt") || (str5 == "dd"))))
                        {
                            string str6;
                            if (((str6 = node.TagName) != null) && (((str6 == "li") || (str6 == "option")) || ((str6 == "dt") || (str6 == "dd"))))
                            {
                                node2.Parent.Add(node);
                            }
                            else
                            {
                                node2.Add(node);
                            }
                        }
                        else
                        {
                            node2.Add(node);
                        }
                    }
                    if (!flag2)
                    {
                        string tagName = node.TagName;
                        if (tagName != null)
                        {
                            int num5;
                            Dictionary<string, int> c = null;
                            if (c == null)
                            {
                                Dictionary<string, int> dictionary1 = new Dictionary<string, int>(6);
                                dictionary1.Add("br", 0);
                                dictionary1.Add("hr", 1);
                                dictionary1.Add("img", 2);
                                dictionary1.Add("input", 3);
                                dictionary1.Add("meta", 4);
                                dictionary1.Add("link", 5);
                                c = dictionary1;
                            }
                            if (c.TryGetValue(tagName, out num5))
                            {
                                switch (num5)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                    case 5:
                                        node.IsComplete = true;
                                        goto Label_0343;
                                }
                            }
                        }
                        node2 = node;
                    }
                    else
                    {
                        node.IsComplete = true;
                    }
                }
            Label_0343:
                str3 = "";
                if (str.Equals("style"))
                {
                    int num3 = this.b.ToLower().IndexOf("</style", startIndex);
                    if (num3 != -1)
                    {
                        str3 = this.b.Substring(startIndex, num3 - startIndex);
                        startIndex = this.b.IndexOf(">", num3);
                    }
                    else
                    {
                        str3 = this.b.Substring(startIndex);
                        startIndex = this.b.Length;
                    }
                    node2 = node.Parent;
                }
                else if (str.Equals("script"))
                {
                    int num4 = this.b.ToLower().IndexOf("</script", startIndex);
                    if (num4 != -1)
                    {
                        str3 = this.b.Substring(startIndex, num4 - startIndex);
                        startIndex = this.b.IndexOf(">", num4);
                    }
                    else
                    {
                        str3 = this.b.Substring(startIndex);
                        startIndex = this.b.Length;
                    }
                    node2 = node.Parent;
                }
                else if (this.b.IndexOf("<", startIndex) != -1)
                {
                    str3 = this.b.Substring(startIndex, this.b.IndexOf("<", startIndex) - startIndex);
                }
                else
                {
                    str3 = this.b.Substring(startIndex);
                    startIndex = this.b.Length;
                }
                if (!node.IsComplete)
                {
                    node.Text = node.Text + str3;
                }
                else if (GetCleanText(str3).Length > 0)
                {
                    node = new HTMLNode("font") {
                        Text = str3
                    };
                    if (node2 == null)
                    {
                        this.c.Nodes.Add(node);
                    }
                    else
                    {
                        node2.Add(node);
                    }
                }
                if (node.TagName.Equals("style") || node.TagName.Equals("script"))
                {
                    node.IsComplete = true;
                }
            }
            if (startIndex < this.b.Length)
            {
                node = new HTMLNode("font") {
                    Text = this.b.Substring(startIndex, this.b.Length - startIndex)
                };
                this.c.Nodes.Add(node);
                node.IsComplete = true;
            }
        }

        private void da(HTMLNode A_0)
        {
            for (int i = A_0.Nodes.Count - 1; i >= 0; i--)
            {
                if (A_0.Nodes[i].Nodes.Count == 0)
                {
                    if (A_0.Nodes[i].TagName.Equals("font") && GetCleanText(A_0.Nodes[i].Text).Trim().Equals(string.Empty))
                    {
                        string tagName = "";
                        if (i == 0)
                        {
                            tagName = A_0.Nodes[1].TagName;
                        }
                        else
                        {
                            tagName = A_0.Nodes[i - 1].TagName;
                        }
                        if ((tagName.Equals("td") || tagName.Equals("th")) || tagName.Equals("tr"))
                        {
                            A_0.Nodes.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    this.da(A_0.Nodes[i]);
                }
            }
        }

        private static int da(string A_0)
        {
            int num = 0;
            for (int i = A_0.Length - 1; i >= 0; i--)
            {
                int num3 = A_0[i];
                if ((num3 >= 0x30) && (num3 <= 0x39))
                {
                    num3 -= 0x30;
                }
                else if ((num3 >= 0x61) && (num3 <= 0x66))
                {
                    num3 -= 0x57;
                }
                else
                {
                    num3 = 0;
                }
                num3 *= (int) Math.Pow(16.0, (double) ((A_0.Length - i) - 1));
                num += num3;
            }
            return num;
        }

        private HTMLNode da(HTMLNode A_0, string A_1)
        {
            if (A_0.TagName.Equals(A_1.ToLower()))
            {
                return A_0;
            }
            HTMLNode node = null;
            for (int i = 0; i < A_0.Nodes.Count; i++)
            {
                node = this.da(A_0.Nodes[i], A_1);
                if (node != null)
                {
                    return node;
                }
            }
            return node;
        }

        public static Alignment AlignFromHtml(string align)
        {
            switch (align.ToLower())
            {
                case "center":
                    return Alignment.Center;

                case "left":
                    return Alignment.Left;

                case "right":
                    return Alignment.Right;
            }
            return Alignment.Default;
        }

        private void db(HTMLNode A_0, string A_1)
        {
            string str = "";
            string str2 = "";
            string input = new Regex(@"\s*=\s*", RegexOptions.Singleline | RegexOptions.IgnoreCase).Replace(A_1, "=");
            MatchCollection matchs = new Regex("((\\w+)=((\"[^\"]*\")|('[^']*')|([^\"^' ]+)))|readonly|checked|selected", RegexOptions.Singleline | RegexOptions.IgnoreCase).Matches(input);
            for (int i = 0; i < matchs.Count; i++)
            {
                if (matchs[i].Value.IndexOf('=') > 0)
                {
                    str = matchs[i].Value.Substring(0, matchs[i].Value.IndexOf("=")).Trim();
                    str2 = matchs[i].Value.Substring(matchs[i].Value.IndexOf("=") + 1).Trim();
                    if (str2.Length > 0)
                    {
                        if (str2[0] == '\'')
                        {
                            str2 = str2.Replace("'", "");
                        }
                        else if (str2[0] == '"')
                        {
                            str2 = str2.Replace("\"", "");
                        }
                    }
                }
                else
                {
                    if (matchs[i].Value.Trim().Equals("/"))
                    {
                        continue;
                    }
                    str = matchs[i].Value.Trim();
                    str2 = "";
                }
                if (str.Trim().Length > 0)
                {
                    A_0.Param.Add(str.ToLower(), str2);
                }
            }
        }

        public static BorderStyle BorderStyleFromParam(string param, BorderStyle style)
        {
            string[] strArray = param.Trim().Split(new char[] { ' ' });
            if (strArray.Length > 2)
            {
                Color color = ColorFromHTML(strArray[2]);
                style.BorderColor = color;
            }
            if (strArray.Length > 1)
            {
                style.Type = BorderTypeFromHTML(strArray[1]);
            }
            if (strArray.Length > 0)
            {
                style.Width = FontSizeFromHTML(strArray[0]);
            }
            return style;
        }

        public static BorderType BorderTypeFromHTML(string html)
        {
            string key = html.Trim().ToUpper();
            if (key != null)
            {
                int num;
                Dictionary<string, int> c = new Dictionary<string, int>();
                if (c.Count == 0)
                {
                    Dictionary<string, int> dictionary1 = new Dictionary<string, int>(10);
                    dictionary1.Add("NONE", 0);
                    dictionary1.Add("HIDDEN", 1);
                    dictionary1.Add("DOTTED", 2);
                    dictionary1.Add("DASHED", 3);
                    dictionary1.Add("SOLID", 4);
                    dictionary1.Add("DOUBLIE", 5);
                    dictionary1.Add("GROOVE", 6);
                    dictionary1.Add("RIDGE", 7);
                    dictionary1.Add("INSERT", 8);
                    dictionary1.Add("OUTSET", 9);
                    c = dictionary1;
                }
                if (c.TryGetValue(key, out num))
                {
                    switch (num)
                    {
                        case 0:
                            return BorderType.NONE;

                        case 1:
                            return BorderType.HIDDEN;

                        case 2:
                            return BorderType.DOTTED;

                        case 3:
                            return BorderType.DASHED;

                        case 4:
                            return BorderType.SOLID;

                        case 5:
                            return BorderType.DOUBLIE;

                        case 6:
                            return BorderType.GROOVE;

                        case 7:
                            return BorderType.RIDGE;

                        case 8:
                            return BorderType.INSERT;

                        case 9:
                            return BorderType.OUTSET;
                    }
                }
            }
            return BorderType.NONE;
        }

        public static Color ColorFromHTML(string color)
        {
            if (color.IndexOf("#") == -1)
            {
                return TypeExtensions.FromName(color);
            }
            return ColorFromRGBStr(color);
        }

        public static Color ColorFromRGBStr(string rgbstr)
        {
            int red = 0;
            int green = 0;
            int blue = 0;
            if (rgbstr.StartsWith("#") && (rgbstr.Length == 7))
            {
                string str = rgbstr.ToLower();
                red = da(str.Substring(1, 2));
                green = da(str.Substring(3, 2));
                blue = da(str.Substring(5, 2));
            }
            return Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
        }

        public static FormFieldType FieldType(HTMLNode node)
        {
            switch (node.TagName)
            {
                case "form":
                    return FormFieldType.Form;

                case "input":
                {
                    string str2;
                    Param param = node.Param.ByName("type");
                    if ((param != null) && ((str2 = param.Value.ToLower()) != null))
                    {
                        int num;
                        Dictionary<string, int> c = new Dictionary<string, int>();
                        if (c.Count == 0)
                        {
                            Dictionary<string, int> dictionary1 = new Dictionary<string, int>(8);
                            dictionary1.Add("text", 0);
                            dictionary1.Add("password", 1);
                            dictionary1.Add("submit", 2);
                            dictionary1.Add("reset", 3);
                            dictionary1.Add("checkbox", 4);
                            dictionary1.Add("radio", 5);
                            dictionary1.Add("hidden", 6);
                            dictionary1.Add("image", 7);
                            c = dictionary1;
                        }
                        if (c.TryGetValue(str2, out num))
                        {
                            switch (num)
                            {
                                case 0:
                                    return FormFieldType.Text;

                                case 1:
                                    return FormFieldType.Password;

                                case 2:
                                    return FormFieldType.Submit;

                                case 3:
                                    return FormFieldType.Reset;

                                case 4:
                                    return FormFieldType.CheckBox;

                                case 5:
                                    return FormFieldType.Radio;

                                case 6:
                                    return FormFieldType.Hidden;

                                case 7:
                                    return FormFieldType.Image;
                            }
                        }
                    }
                    return FormFieldType.None;
                }
                case "textarea":
                    return FormFieldType.TextArea;

                case "select":
                    return FormFieldType.Select;

                case "option":
                    return FormFieldType.Option;
            }
            return FormFieldType.None;
        }

        public HTMLNode FindNodeByTagName(string tagName)
        {
            HTMLNode node = null;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                node = this.da(this.Nodes[i], tagName);
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }

        public static int FontSizeFromHTML(string fontsize)
        {
            double defaultFontSize = 12.0;
            try
            {
                if (fontsize.IndexOf("pt") != -1)
                {
                    if (fontsize == "6pt")
                    {
                        defaultFontSize = 8.0;
                    }
                    else if (fontsize == "6pt")
                    {
                        defaultFontSize = 8.0;
                    }
                    else if (fontsize == "7pt")
                    {
                        defaultFontSize = 9.0;
                    }
                    else if (fontsize == "7.5pt")
                    {
                        defaultFontSize = 10.0;
                    }
                    else if (fontsize == "8pt")
                    {
                        defaultFontSize = 11.0;
                    }
                    else if (fontsize == "9pt")
                    {
                        defaultFontSize = 12.0;
                    }
                    else if (fontsize == "10pt")
                    {
                        defaultFontSize = 13.0;
                    }
                    else if (fontsize == "10.5pt")
                    {
                        defaultFontSize = 14.0;
                    }
                    else if (fontsize == "11pt")
                    {
                        defaultFontSize = 15.0;
                    }
                    else if (fontsize == "12pt")
                    {
                        defaultFontSize = 16.0;
                    }
                    else if (fontsize == "13pt")
                    {
                        defaultFontSize = 17.0;
                    }
                    else if (fontsize == "13.5pt")
                    {
                        defaultFontSize = 18.0;
                    }
                    else if (fontsize == "14pt")
                    {
                        defaultFontSize = 19.0;
                    }
                    else if (fontsize == "14.5pt")
                    {
                        defaultFontSize = 20.0;
                    }
                    else if (fontsize == "15pt")
                    {
                        defaultFontSize = 21.0;
                    }
                    else if (fontsize == "16pt")
                    {
                        defaultFontSize = 22.0;
                    }
                    else if (fontsize == "17pt")
                    {
                        defaultFontSize = 23.0;
                    }
                    else if (fontsize == "18pt")
                    {
                        defaultFontSize = 24.0;
                    }
                    else if (fontsize == "20pt")
                    {
                        defaultFontSize = 26.0;
                    }
                    else if (fontsize == "22pt")
                    {
                        defaultFontSize = 29.0;
                    }
                    else if (fontsize == "24pt")
                    {
                        defaultFontSize = 32.0;
                    }
                    else if (fontsize == "26pt")
                    {
                        defaultFontSize = 35.0;
                    }
                    else if (fontsize == "27pt")
                    {
                        defaultFontSize = 36.0;
                    }
                    else if (fontsize == "28pt")
                    {
                        defaultFontSize = 37.0;
                    }
                    else if (fontsize == "29pt")
                    {
                        defaultFontSize = 38.0;
                    }
                    else if (fontsize == "30pt")
                    {
                        defaultFontSize = 40.0;
                    }
                    else if (fontsize == "32pt")
                    {
                        defaultFontSize = 42.0;
                    }
                    else if (fontsize == "34pt")
                    {
                        defaultFontSize = 45.0;
                    }
                    else if (fontsize == "36pt")
                    {
                        defaultFontSize = 48.0;
                    }
                    else
                    {
                        defaultFontSize = Convert.ToDouble(fontsize.Replace("pt", "")) * 1.2;
                    }
                }
                else if (fontsize.IndexOf("px") != -1)
                {
                    defaultFontSize = Convert.ToDouble(fontsize.Replace("px", ""));
                }
                else if (int.Parse(fontsize).ToString() == fontsize)
                {
                    defaultFontSize = int.Parse(fontsize);
                }
                else
                {
                    defaultFontSize = Convert.ToDouble(fontsize);
                }
            }
            catch
            {
                defaultFontSize = HTMLTree.defaultFontSize;
            }
            return (int) defaultFontSize;
        }

        public static string GetCleanText(string text)
        {
            Regex regex = new Regex(@"[\f\n\r\t\v]", RegexOptions.Singleline);
            return regex.Replace(text, "");
        }

        public static string HTMLDecode(string html)
        {
            Regex regex = new Regex(" +", RegexOptions.Singleline);
            char ch = '\x00a2';
            char ch2 = '\x00e7';
            return regex.Replace(html, " ").Replace("&nbsp;", " ").Replace("&#160;", " ").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&copy;", ch.ToString()).Replace("&reg;", ch2.ToString());
        }

        public static bool IsCss2(string name)
        {
            return (":align:azimuth:background:background-attachment:background-color:background-image:background-position:background-repeat:border:border-collapse:border-color:border-spacing:border-style:border-top:border-right:border-bottom:border-left:border-top-color:border-right-color:border-bottom-color:border-left-color:border-top-style:border-right-style:border-bottom-style:border-left-style:border-top-width:border-right-width:border-bottom-width:border-left-width:border-width:bottom:caption-side:clear:clip:color:content:counter-increment:counter-reset:cue:cue-after:cue-before:cursor:direction:display:elevation:empty-cells:float:font:font-family:font-size:font-size-adjust:font-stretch:font-style:font-variant:font-weight:height:left:letter-spacing:line-height:list-style:list-style-image:list-style-position:list-style-type:margin:margin-top:margin-right:margin-bottom:margin-left:marker-offset:marks:max-height:max-width:min-height:min-width:orphans:outline:outline-color:outline-style:outline-width:overflow:padding:padding-top:padding-right:padding-bottom:padding-left:page:page-break-after:page-break-before:page-break-inside:pause:pause-after:pause-before:pitch:pitch-range:play-during:position:quotes:richness:right:size:speak:speak-header:speak-numeral:speak-punctuation:speech-rate:stress:table-layout:text-align:text-decoration:text-indent:text-shadow:text-transform:top:unicode-bidi:vertical-align:visibility:voice-family:volume:white-space:widows:width:word-spacing:z-index:".IndexOf(string.Format(":{0}:", name)) != -1);
        }

        public static bool IsForm(string tagname)
        {
            if ((!tagname.Equals("form") && !tagname.Equals("input")) && (!tagname.Equals("textarea") && !tagname.Equals("select")))
            {
                return tagname.Equals("option");
            }
            return true;
        }

        public static int NodeWidth(HTMLNode node)
        {
            int num = node.Css.Css.IndexOf("width", CssType.ForAll, "");
            int num2 = 0;
            if (num != -1)
            {
                string fontsize = node.Css.Css[num].Value.Trim();
                if (!fontsize.EndsWith("%"))
                {
                    num2 = FontSizeFromHTML(fontsize);
                }
                return num2;
            }
            Param param = node.Param.ByName("width");
            if ((param != null) && !param.Value.EndsWith("%"))
            {
                num2 = FontSizeFromHTML(param.Value);
            }
            return num2;
        }

        public static void ParseColStyleFromHTML(HTMLNode node, TableCol col)
        {
            ParseRectStyleFromHTML(node, col.Rect);
            Param param = node.Param.ByName("colspan");
            if (param != null)
            {
                col.ColSpan = int.Parse(param.Value.Trim());
            }
            else
            {
                col.ColSpan = 1;
            }
            param = node.Param.ByName("rowspan");
            if (param != null)
            {
                col.RowSpan = int.Parse(param.Value.Trim()) + 1;
                col.IsUsed = false;
            }
        }

        public static void ParseDivStyle(HTMLNode node, DivPos div)
        {
            for (int i = 0; i < node.Css.Css.Count; i++)
            {
                int num2;
                int num3;
                string str = node.Css.Css[i].Name.ToLower();
                if (str != null)
                {
                    if (!(str == "position"))
                    {
                        if (str == "left")
                        {
                            goto Label_007A;
                        }
                        if (str == "top")
                        {
                            goto Label_009F;
                        }
                    }
                    else
                    {
                        div.PosType = PostionTypeFromHTML(node.Css.Css[i].Value.Trim());
                    }
                }
                continue;
            Label_007A:
                num2 = FontSizeFromHTML(node.Css.Css[i].Value);
                div.Left = num2;
                continue;
            Label_009F:
                num3 = FontSizeFromHTML(node.Css.Css[i].Value);
                div.Top = num3;
            }
        }

        public static void ParseRectStyleFromHTML(HTMLNode node, RectStyle rect)
        {
            Param param = node.Param.ByName("width");
            string fontsize = "";
            if (param != null)
            {
                fontsize = param.Value.Trim();
            }
            else
            {
                int num = node.Css.Css.IndexOf("width", CssType.ForAll, "");
                if (num != -1)
                {
                    fontsize = node.Css.Css[num].Value.Trim();
                }
            }
            if (fontsize != string.Empty)
            {
                if (fontsize.EndsWith("%"))
                {
                    rect.Width = int.Parse(fontsize.Substring(0, fontsize.Length - 1));
                    rect.WidthType = LengthType.Rate;
                }
                else
                {
                    rect.Width = FontSizeFromHTML(fontsize);
                    rect.OrginWidth = rect.Width;
                    rect.WidthType = LengthType.Length;
                }
            }
            else
            {
                rect.Width = -1;
                rect.OrginWidth = -1;
            }
            param = node.Param.ByName("height");
            fontsize = "";
            if (param != null)
            {
                fontsize = param.Value.Trim();
            }
            else
            {
                int num2 = node.Css.Css.IndexOf("height", CssType.ForAll, "");
                if (num2 != -1)
                {
                    fontsize = node.Css.Css[num2].Value.Trim();
                }
            }
            if (fontsize != string.Empty)
            {
                if (fontsize.EndsWith("%"))
                {
                    rect.Height = int.Parse(fontsize.Substring(0, fontsize.Length - 1));
                    rect.HeightType = LengthType.Rate;
                }
                else
                {
                    rect.Height = FontSizeFromHTML(fontsize);
                    rect.HeightType = LengthType.Length;
                }
            }
            for (int i = 0; i < node.Css.Css.Count; i++)
            {
                string key = node.Css.Css[i].Name.ToLower();
                if (key != null)
                {
                    int num6;
                    Dictionary<string, int> c = null;
                    if (c == null)
                    {
                        Dictionary<string, int> dictionary1 = new Dictionary<string, int>(0x1a);
                        dictionary1.Add("background-color", 0);
                        dictionary1.Add("border", 1);
                        dictionary1.Add("border-left", 2);
                        dictionary1.Add("border-right", 3);
                        dictionary1.Add("border-top", 4);
                        dictionary1.Add("border-bottom", 5);
                        dictionary1.Add("border-width", 6);
                        dictionary1.Add("border-type", 7);
                        dictionary1.Add("border-color", 8);
                        dictionary1.Add("border-top-color", 9);
                        dictionary1.Add("border-right-color", 10);
                        dictionary1.Add("border-bottom-color", 11);
                        dictionary1.Add("border-left-color", 12);
                        dictionary1.Add("border-top-style", 13);
                        dictionary1.Add("border-right-style", 14);
                        dictionary1.Add("border-bottom-style", 15);
                        dictionary1.Add("border-left-style", 0x10);
                        dictionary1.Add("border-top-width", 0x11);
                        dictionary1.Add("border-right-width", 0x12);
                        dictionary1.Add("border-bottom-width", 0x13);
                        dictionary1.Add("border-left-width", 20);
                        dictionary1.Add("margin", 0x15);
                        dictionary1.Add("margin-bottom", 0x16);
                        dictionary1.Add("margin-left", 0x17);
                        dictionary1.Add("margin-right", 0x18);
                        dictionary1.Add("margin-top", 0x19);
                        c = dictionary1;
                    }
                    if (c.TryGetValue(key, out num6))
                    {
                        BorderStyle style;
                        int num4;
                        BorderType type;
                        Color color2;
                        switch (num6)
                        {
                            case 0:
                            {
                                Color color = ColorFromHTML(node.Css.Css[i].Value.Trim());
                                rect.BGColor = color;
                                break;
                            }
                            case 1:
                                style = BorderStyleFromParam(node.Css.Css[i].Value, rect.TopBorder);
                                rect.SetBorderColor(style.BorderColor);
                                rect.SetBorderType(style.Type);
                                rect.SetBorderWidth(style.Width);
                                break;

                            case 2:
                                style = BorderStyleFromParam(node.Css.Css[i].Value, rect.LeftBorder);
                                rect.LeftBorder.BorderColor = style.BorderColor;
                                rect.LeftBorder.Type = style.Type;
                                rect.LeftBorder.Width = style.Width;
                                break;

                            case 3:
                                style = BorderStyleFromParam(node.Css.Css[i].Value, rect.RightBorder);
                                rect.RightBorder.BorderColor = style.BorderColor;
                                rect.RightBorder.Type = style.Type;
                                rect.RightBorder.Width = style.Width;
                                break;

                            case 4:
                                style = BorderStyleFromParam(node.Css.Css[i].Value, rect.TopBorder);
                                rect.TopBorder.BorderColor = style.BorderColor;
                                rect.TopBorder.Type = style.Type;
                                rect.TopBorder.Width = style.Width;
                                break;

                            case 5:
                                style = BorderStyleFromParam(node.Css.Css[i].Value, rect.BottomBorder);
                                rect.BottomBorder.BorderColor = style.BorderColor;
                                rect.BottomBorder.Type = style.Type;
                                rect.BottomBorder.Width = style.Width;
                                break;

                            case 6:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.SetBorderWidth(num4);
                                break;

                            case 7:
                                type = BorderTypeFromHTML(node.Css.Css[i].Value);
                                rect.SetBorderType(type);
                                break;

                            case 8:
                                color2 = ColorFromHTML(node.Css.Css[i].Value);
                                rect.SetBorderColor(color2);
                                break;

                            case 9:
                                color2 = ColorFromHTML(node.Css.Css[i].Value);
                                rect.TopBorder.BorderColor = color2;
                                break;

                            case 10:
                                color2 = ColorFromHTML(node.Css.Css[i].Value);
                                rect.RightBorder.BorderColor = color2;
                                break;

                            case 11:
                                color2 = ColorFromHTML(node.Css.Css[i].Value);
                                rect.BottomBorder.BorderColor = color2;
                                break;

                            case 12:
                                color2 = ColorFromHTML(node.Css.Css[i].Value);
                                rect.LeftBorder.BorderColor = color2;
                                break;

                            case 13:
                                type = BorderTypeFromHTML(node.Css.Css[i].Value);
                                rect.TopBorder.Type = type;
                                break;

                            case 14:
                                type = BorderTypeFromHTML(node.Css.Css[i].Value);
                                rect.RightBorder.Type = type;
                                break;

                            case 15:
                                type = BorderTypeFromHTML(node.Css.Css[i].Value);
                                rect.BottomBorder.Type = type;
                                break;

                            case 0x10:
                                type = BorderTypeFromHTML(node.Css.Css[i].Value);
                                rect.LeftBorder.Type = type;
                                break;

                            case 0x11:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.TopBorder.Width = num4;
                                break;

                            case 0x12:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.RightBorder.Width = num4;
                                break;

                            case 0x13:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.BottomBorder.Width = num4;
                                break;

                            case 20:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.LeftBorder.Width = num4;
                                break;

                            case 0x15:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.SetMargin(num4);
                                break;

                            case 0x16:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.BottomBorder.Width = num4;
                                break;

                            case 0x17:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.LeftBorder.Width = num4;
                                break;

                            case 0x18:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.RightBorder.Margin = num4;
                                break;

                            case 0x19:
                                num4 = FontSizeFromHTML(node.Css.Css[i].Value);
                                rect.TopBorder.Margin = num4;
                                break;
                        }
                    }
                }
            }
            Param param2 = node.Param.ByName("bgcolor");
            if (param2 != null)
            {
                Color color3 = ColorFromHTML(param2.Value.Trim());
                rect.BGColor = color3;
            }
            Param param3 = node.Param.ByName("align");
            if (param3 != null)
            {
                Alignment alignment = AlignFromHtml(param3.Value.Trim());
                rect.Align = alignment;
            }
            Param param4 = node.Param.ByName("border");
            if (param4 != null)
            {
                int width = FontSizeFromHTML(param4.Value.Trim());
                rect.SetBorderWidth(width);
                if (width > 0)
                {
                    rect.SetBorderType(BorderType.SOLID);
                }
            }
            Param param5 = node.Param.ByName("bordercolor");
            if (param5 != null)
            {
                Color color4 = ColorFromHTML(param5.Value.Trim());
                rect.SetBorderColor(color4);
            }
        }

        public static PositionType PostionTypeFromHTML(string postion)
        {
            switch (postion.ToLower())
            {
                case "static":
                    return PositionType.Static;

                case "absolute":
                    return PositionType.Absolute;

                case "fixed":
                    return PositionType.Fixed;

                case "relative":
                    return PositionType.Relative;
            }
            return PositionType.Static;
        }

        public HTMLNode BodyNode
        {
            get
            {
                if (this.d == null)
                {
                    this.d = this.FindNodeByTagName("body");
                }
                return this.d;
            }
        }

        public HTMLNode HeadNode
        {
            get
            {
                if (this.e == null)
                {
                    this.e = this.FindNodeByTagName("head");
                }
                return this.e;
            }
        }

        public HTMLNodes Nodes
        {
            get
            {
                return this.c.Nodes;
            }
        }

        public HTMLNode RootNode
        {
            get
            {
                return this.c;
            }
        }

        public string Title
        {
            get
            {
                if (this.TitleNode != null)
                {
                    return this.TitleNode.Text;
                }
                return "";
            }
        }

        public HTMLNode TitleNode
        {
            get
            {
                if ((this.f == null) && (this.HeadNode != null))
                {
                    this.f = this.da(this.HeadNode, "title");
                }
                return this.f;
            }
        }
    }
}

