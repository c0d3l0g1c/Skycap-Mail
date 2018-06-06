// Type: CuteEditor.Convertor.RTF.HTML2RTF
// Assembly: CuteEditor, Version=6.6.0.0, Culture=neutral, PublicKeyToken=3858aa6802b1223a
// Assembly location: C:\Users\Public\Downloads\CuteEditor_for_NET6\Framework 2.0\bin\CuteEditor.dll

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Skycap.Text.Converters.Html;
using Skycap.Text.Converters.Rtf;
using Windows.UI;

namespace Skycap.Text.Converters
{
    public class HTML2RTF
    {
        private string e = "";
        private HTMLTree a;
        private List<object> b;
        private List<object> c;
        private FontStyle d;

        public string RTF
        {
            get
            {
                return this.e;
            }
        }

        public HTML2RTF(string html)
        {
            this.a = new HTMLTree(html);
            this.d = new FontStyle(HTMLTree.defaultFontName, HTMLTree.defaultFontSize, HTMLTree.defaultColor, Colors.Transparent);
            this.b = new List<object>();
            this.c = new List<object>();
            this.c.Add((object)HTMLTree.defaultFontName);
            this.b.Add((object)HTMLTree.defaultColor);
            this.e = this.f();
        }

        private string f()
        {
            HTMLNode bodyNode = this.a.BodyNode;
            int A_2 = -1;
            return this.de() + this.dc() + this.da() + this.db() + "\\viewkind4\\uc1\\pard" + this.da(this.a.RootNode, new FontStyle(HTMLTree.defaultFontName, HTMLTree.defaultFontSize, HTMLTree.defaultColor, Colors.Transparent), ref A_2) + this.dd();
        }

        private string da(HTMLNode A_0, FontStyle A_1, ref int A_2)
        {
            if (A_0.TagName.Equals("head"))
                return "";
            string str = "";
            FontStyle fontStyle = A_1;
            int A_2_1 = -1;
            int index1 = A_0.Css.Css.IndexOf("font-size", CssType.ForAll, "");
            if (index1 == -1)
            {
                int index2 = A_0.Css.Css.IndexOf("size", CssType.ForAll, "");
                if (index2 != -1)
                {
                    string fontsize = A_0.Css.Css[index2].Value.Trim();
                    fontStyle.FontSize = (uint)HTMLTree.FontSizeFromHTML(fontsize);
                    fontStyle.FontSize = (uint)((((int)fontStyle.FontSize - 1) * 2 + 10) * 2);
                }
            }
            else
            {
                string fontsize = A_0.Css.Css[index1].Value.Trim();
                fontStyle.FontSize = (uint)(HTMLTree.FontSizeFromHTML(fontsize) * 2);
            }
            int index3 = A_0.Css.Css.IndexOf("font-family", CssType.ForAll, "");
            if (index3 != -1)
                fontStyle.FontName = A_0.Css.Css[index3].Value.Trim();
            int index4 = A_0.Css.Css.IndexOf("color", CssType.ForAll, "");
            if (index4 != -1)
            {
                string color = A_0.Css.Css[index4].Value.Trim();
                fontStyle.FontColor = HTMLTree.ColorFromHTML(color);
            }
            int index5 = A_0.Css.Css.IndexOf("align", CssType.ForAll, "");
            if (index5 != -1)
            {
                string align = A_0.Css.Css[index5].Value.Trim();
                fontStyle.Align = HTMLTree.AlignFromHtml(align);
            }
            switch (A_0.TagName)
            {
                case "i":
                case "em":
                    fontStyle.IsItalic = true;
                    break;
                case "b":
                case "strong":
                    fontStyle.IsBold = true;
                    break;
                case "u":
                    fontStyle.IsUnderLine = true;
                    break;
                case "center":
                    fontStyle.Align = Alignment.Center;
                    break;
                case "ul":
                    A_2_1 = 0;
                    break;
                case "ol":
                    A_2_1 = 1;
                    break;
                case "sup":
                    fontStyle.Raise = RaiseType.Sup;
                    str = str + "{";
                    break;
                case "sub":
                    fontStyle.Raise = RaiseType.Sub;
                    str = str + "{";
                    break;
            }
            switch (A_0.TagName)
            {
                case "br":
                    str = str + "\\par";
                    break;
                case "li":
                    if (A_2 == 0)
                    {
                        str = str + "\\par\n" + this.da(fontStyle) + "{\\pntext\\'B7\\tab}" + this.da(A_0.Text);
                        break;
                    }
                    else if (A_2 > 0)
                    {
                        str = str + "\\par\n " + this.da(fontStyle) + "{\\pntext " + A_2.ToString() + "\\tab}" + this.da(A_0.Text);
                        ++A_2;
                        break;
                    }
                    else
                        break;
                case "font":
                    if (!A_0.Text.Trim().Equals(string.Empty))
                    {
                        str = str + this.da(fontStyle) + " " + this.da(A_0.Text);
                        break;
                    }
                    else
                        break;
                case "p":
                case "div":
                    str = str + "\\par\n\\pard" + this.da(fontStyle) + " " + this.da(A_0.Text);
                    break;
                case "img":
                    str = str + RTFImageHelper.LoadFromHtml(A_0, "");
                    break;
                default:
                    if (A_0.Text != string.Empty)
                    {
                        str = str + this.da(fontStyle) + " " + this.da(A_0.Text);
                        break;
                    }
                    else
                        break;
            }
            for (int index2 = 0; index2 < A_0.Nodes.Count; ++index2)
                str = str + this.da(A_0.Nodes[index2], fontStyle, ref A_2_1);
            switch (A_0.TagName)
            {
                case "sup":
                case "sub":
                    str = str + "}";
                    break;
            }
            return str;
        }

        private string da(FontStyle A_0)
        {
            int num1 = this.c.IndexOf((object)A_0.FontName);
            if (num1 == -1)
            {
                this.c.Add((object)A_0.FontName);
                num1 = this.c.Count - 1;
            }
            int num2 = this.b.IndexOf((object)A_0.FontColor);
            if (num2 == -1)
            {
                this.b.Add((object)A_0.FontColor);
                num2 = this.b.Count - 1;
            }
            string str1 = string.Format("\\cf{0}\\f{1}\\fs{2}", (object)(num2 + 1), (object)num1, (object)A_0.FontSize);
            string str2;
            switch (A_0.Align)
            {
                case Alignment.Default:
                    str2 = str1 + "\\qj";
                    break;
                case Alignment.Left:
                    str2 = str1 + "\\ql";
                    break;
                case Alignment.Right:
                    str2 = str1 + "\\qr";
                    break;
                case Alignment.Center:
                    str2 = str1 + "\\qc";
                    break;
                default:
                    str2 = str1 + "\\qj";
                    break;
            }
            switch (A_0.Raise)
            {
                case RaiseType.Sub:
                    str2 = str2 + "\\sub";
                    break;
                case RaiseType.Sup:
                    str2 = str2 + "\\super";
                    break;
            }
            string str3 = !A_0.IsBold ? str2 + "\\b0" : str2 + "\\b1";
            string str4 = !A_0.IsItalic ? str3 + "\\i0" : str3 + "\\i1";
            return !A_0.IsUnderLine ? str4 + "\\ulnone" : str4 + "\\ul";
        }

        private string de()
        {
            return "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033";
        }

        private string dd()
        {
            return "}";
        }

        private string dc()
        {
            string str = "{\\colortbl;";
            for (int index = 0; index < this.b.Count; ++index)
            {
                Color color = (Color)this.b[index];
                str = str + string.Format("\\red{0}\\green{1}\\blue{2};", (object)color.R, (object)color.G, (object)color.B);
            }
            return str + "}";
        }

        private string db()
        {
            return "{\\*\\generator CuteEditor 5.0;}";
        }

        private string da()
        {
            string str = "";
            for (int index = 0; index < this.c.Count; ++index)
            {
                this.da((string)this.c[index]);
                str = str + string.Format("{{\\f{0}\\fswiss\\fcharset{1} {2};}}\n", (object)index, (object)Encoding.UTF8.WebName, (object)this.da((string)this.c[index]));
            }
            return string.Format("{{\\fonttbl\n{0}}}", (object)str);
        }

        private string da(string A_0)
        {
            string s = HTMLTree.HTMLDecode(A_0);
            string str = "";
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            for (int index = 0; index < bytes.Length; ++index)
                str = (int)bytes[index] == 92 || (int)bytes[index] == 123 || (int)bytes[index] == 125 ? str + "\\" + ((char)bytes[index]).ToString() : ((int)bytes[index] <= 128 ? str + ((char)bytes[index]).ToString() : str + "\\'" + ((int)bytes[index]).ToString("x"));
            return str;
        }
    }
}
