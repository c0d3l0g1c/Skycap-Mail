namespace Skycap.Text.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Skycap.Text.Converters.Html;
    using Skycap.Text.Converters.Rtf;
    using Windows.UI;

    public class RTF2HTML
    {
        private List<object> a = new List<object>(5);
        private List<object> b = new List<object>(5);
        private List<object> c = new List<object>();
        private FontStyle d;
        public RTFTree tree = new RTFTree();

        public RTF2HTML(string str)
        {
            if (str.StartsWith(@"{\rtf"))
            {
                this.tree.LoadRtfText(str);
            }
            else
            {
                this.tree.LoadRTFFile(str);
            }
        }

        private string da()
        {
            string str = "";
            if (this.d.IsUnderLine)
            {
                str = str + "</u>";
            }
            if (this.d.IsItalic)
            {
                str = str + "</i>";
            }
            if (this.d.IsBold)
            {
                str = str + "</b>";
            }
            switch (this.d.Raise)
            {
                case RaiseType.Sub:
                    str = str + "</sub>";
                    break;

                case RaiseType.Sup:
                    str = str + "</sup>";
                    break;

                default:
                    if (((this.da(this.d.FontColor) == "#000000") && (this.d.FontSize == HTMLTree.defaultFontSize)) && (this.d.FontName.ToLower() == "times new roman"))
                    {
                        str = str + "";
                    }
                    else
                    {
                        str = str + "</span>";
                    }
                    break;
            }
            if (this.d.Align == Alignment.Default)
            {
                return str;
            }
            return (str + "</p>");
        }

        private void da(List<object> A_0)
        {
            RTFTreeNode firstChild = this.tree.RootNode.FirstChild;
            int num = 0;
            RTFTreeNode node3 = null;
            while (num < firstChild.Children.Count)
            {
                if ((firstChild.Children[num].ObjectType == RTFObjectType.Group) && firstChild.Children[num].FirstChild.Key.Equals("colortbl"))
                {
                    node3 = firstChild.Children[num];
                    break;
                }
                num++;
            }
            if (node3 != null)
            {
                int red = 0;
                int green = 0;
                int blue = 0;
                for (int i = 1; i < node3.Children.Count; i++)
                {
                    RTFTreeNode node4 = node3.Children[i];
                    if ((node4.ObjectType == RTFObjectType.Text) && node4.Key.StartsWith(";"))
                    {
                        A_0.Add(Color.FromArgb(255, (byte)red, (byte)green, (byte)blue));
                        red = 0;
                        green = 0;
                        blue = 0;
                    }
                    else
                    {
                        string str;
                        if ((node4.ObjectType == RTFObjectType.KeyWord) && ((str = node4.Key) != null))
                        {
                            if (!(str == "red"))
                            {
                                if (str == "green")
                                {
                                    goto Label_0120;
                                }
                                if (str == "blue")
                                {
                                    goto Label_012B;
                                }
                            }
                            else
                            {
                                red = node4.Param;
                            }
                        }
                    }
                    continue;
                Label_0120:
                    green = node4.Param;
                    continue;
                Label_012B:
                    blue = node4.Param;
                }
            }
        }

        private string da(Color A_0)
        {
            return string.Format("#{0}{1}{2}", this.da(A_0.R, 2), this.da(A_0.G, 2), this.da(A_0.B, 2));
        }

        private Color da(int A_0)
        {
            if (A_0 >= this.b.Count)
            {
                return Colors.Black;
            }
            return (Color) this.b[A_0];
        }

        private string da(string A_0)
        {
            string str = "";
            for (int i = A_0.Length - 1; i >= 0; i--)
            {
                str = str + A_0[i];
            }
            return str;
        }

        private string da(RTFTreeNode A_0, int A_1)
        {
            string str = "";
            RTFTreeNode node = null;
            string str2 = "";
            byte[] bytes = new byte[2];
            int index = 0;
            for (int i = A_1; i < A_0.Children.Count; i++)
            {
                node = A_0.Children[i];
                if ((node.ObjectType != RTFObjectType.Control) || (node.Key != "'"))
                {
                    if (str2 != "")
                    {
                        str = str + this.db(str2);
                    }
                    if (index == 1)
                    {
                        char ch = (char) bytes[0];
                        str = str + this.db(ch.ToString());
                    }
                    index = 0;
                    str2 = "";
                }
                if (node.ObjectType == RTFObjectType.Group)
                {
                    this.c.Add(this.d);
                    this.d = new FontStyle(this.d);
                    str = str + this.da(node, 0);
                    this.d = (FontStyle) this.c[this.c.Count - 1];
                    this.c.RemoveAt(this.c.Count - 1);
                }
                else if (node.ObjectType == RTFObjectType.Control)
                {
                    if (node.Key == "'")
                    {
                        bytes[index] = (byte) node.Param;
                        if (index == 1)
                        {
                            str2 = str2 + Encoding.UTF8.GetString(bytes, 0 , bytes.Length);
                            index = 0;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                else
                {
                    if (node.ObjectType != RTFObjectType.KeyWord)
                    {
                        goto Label_042F;
                    }
                    string key = node.Key;
                    if (key != null)
                    {
                        int num3;
                        Dictionary<string, int> c = null;
                        if (c == null)
                        {
                            Dictionary<string, int> dictionary1 = new Dictionary<string, int>(0x11);
                            dictionary1.Add("f", 0);
                            dictionary1.Add("cf", 1);
                            dictionary1.Add("fs", 2);
                            dictionary1.Add("b", 3);
                            dictionary1.Add("i", 4);
                            dictionary1.Add("ul", 5);
                            dictionary1.Add("ulnone", 6);
                            dictionary1.Add("par", 7);
                            dictionary1.Add("pard", 8);
                            dictionary1.Add("qc", 9);
                            dictionary1.Add("ql", 10);
                            dictionary1.Add("qj", 11);
                            dictionary1.Add("qr", 12);
                            dictionary1.Add("sub", 13);
                            dictionary1.Add("super", 14);
                            dictionary1.Add("ldblquote", 15);
                            dictionary1.Add("rdblquote", 0x10);
                            c = dictionary1;
                        }
                        if (c.TryGetValue(key, out num3))
                        {
                            switch (num3)
                            {
                                case 0:
                                {
                                    this.d.FontName = this.db(node.Param);
                                    continue;
                                }
                                case 1:
                                {
                                    this.d.FontColor = this.da(node.Param);
                                    continue;
                                }
                                case 2:
                                {
                                    this.d.FontSize = (uint) node.Param;
                                    continue;
                                }
                                case 3:
                                    goto Label_031D;

                                case 4:
                                    goto Label_0350;

                                case 5:
                                {
                                    this.d.IsUnderLine = true;
                                    continue;
                                }
                                case 6:
                                {
                                    this.d.IsUnderLine = false;
                                    continue;
                                }
                                case 7:
                                {
                                    str = str + "<br>";
                                    continue;
                                }
                                case 8:
                                {
                                    this.d.Align = Alignment.Default;
                                    continue;
                                }
                                case 9:
                                {
                                    this.d.Align = Alignment.Center;
                                    continue;
                                }
                                case 10:
                                {
                                    this.d.Align = Alignment.Left;
                                    continue;
                                }
                                case 11:
                                {
                                    this.d.Align = Alignment.Default;
                                    continue;
                                }
                                case 12:
                                {
                                    this.d.Align = Alignment.Right;
                                    continue;
                                }
                                case 13:
                                {
                                    this.d.Raise = RaiseType.Sub;
                                    continue;
                                }
                                case 14:
                                {
                                    this.d.Raise = RaiseType.Sup;
                                    continue;
                                }
                                case 15:
                                case 0x10:
                                {
                                    str = str + this.db("&quot;");
                                    continue;
                                }
                            }
                        }
                    }
                }
                continue;
            Label_031D:
                if (!node.HasParam || (node.Param == 1))
                {
                    this.d.IsBold = true;
                }
                else
                {
                    this.d.IsBold = false;
                }
                continue;
            Label_0350:
                if (!node.HasParam || (node.Param == 1))
                {
                    this.d.IsItalic = true;
                }
                else
                {
                    this.d.IsItalic = false;
                }
                continue;
            Label_042F:
                if (node.ObjectType == RTFObjectType.Text)
                {
                    str = str + this.db(node.Key);
                }
            }
            if (str2 != "")
            {
                str = str + this.db(str2);
            }
            return str;
        }

        private string da(int A_0, int A_1)
        {
            return this.dc(A_0).PadLeft(A_1, '0');
        }

        private string db()
        {
            string str = "";
            switch (this.d.Raise)
            {
                case RaiseType.Sub:
                    if ((!(this.da(this.d.FontColor) == "#000000") || (this.d.FontSize != HTMLTree.defaultFontSize)) || !(this.d.FontName.ToLower() == "times new roman"))
                    {
                        str = str + "<sub style=\"";
                        if (this.d.FontSize != HTMLTree.defaultFontSize)
                        {
                            object obj2 = str;
                            str = string.Concat(new object[] { obj2, "font-size:", this.d.FontSize / 2, ";" });
                        }
                        if (this.da(this.d.FontColor) != "#000000")
                        {
                            str = str + "color:" + this.da(this.d.FontColor) + ";";
                        }
                        if (this.d.FontName.ToLower() != "times new roman")
                        {
                            str = str + "font-family:" + this.d.FontName.ToLower() + ";";
                        }
                        str = str + "\">";
                        break;
                    }
                    str = str + "<sub>";
                    break;

                case RaiseType.Sup:
                    if ((!(this.da(this.d.FontColor) == "#000000") || (this.d.FontSize != HTMLTree.defaultFontSize)) || !(this.d.FontName.ToLower() == "times new roman"))
                    {
                        str = str + "<sup style=\"";
                        if (this.d.FontSize != HTMLTree.defaultFontSize)
                        {
                            object obj3 = str;
                            str = string.Concat(new object[] { obj3, "font-size:", this.d.FontSize / 2, ";" });
                        }
                        if (this.da(this.d.FontColor) != "#000000")
                        {
                            str = str + "color:" + this.da(this.d.FontColor) + ";";
                        }
                        if (this.d.FontName.ToLower() != "times new roman")
                        {
                            str = str + "font-family:" + this.d.FontName.ToLower() + ";";
                        }
                        str = str + "\">";
                        break;
                    }
                    str = str + "<sup>";
                    break;

                default:
                    if (((this.da(this.d.FontColor) == "#000000") && (this.d.FontSize == HTMLTree.defaultFontSize)) && (this.d.FontName.ToLower() == "times new roman"))
                    {
                        str = str + "";
                    }
                    else
                    {
                        str = str + "<span style=\"";
                        if (this.d.FontSize != HTMLTree.defaultFontSize)
                        {
                            object obj4 = str;
                            str = string.Concat(new object[] { obj4, "font-size:", this.d.FontSize / 2, ";" });
                        }
                        if (this.da(this.d.FontColor) != "#000000")
                        {
                            str = str + "color:" + this.da(this.d.FontColor) + ";";
                        }
                        if (this.d.FontName.ToLower() != "times new roman")
                        {
                            str = str + "font-family:" + this.d.FontName.ToLower() + ";";
                        }
                        str = str + "\">";
                    }
                    break;
            }
            if (this.d.IsBold)
            {
                str = str + "<b>";
            }
            if (this.d.IsItalic)
            {
                str = str + "<i>";
            }
            if (this.d.IsUnderLine)
            {
                str = str + "<u>";
            }
            if (this.d.Align == Alignment.Default)
            {
                return str;
            }
            return string.Format("<p align='{0}'>{1}", this.d.Align.ToString(), str);
        }

        private void db(List<object> A_0)
        {
            RTFTreeNode firstChild = this.tree.RootNode.FirstChild;
            int num = 0;
            RTFTreeNode node3 = null;
            while (num < firstChild.Children.Count)
            {
                if ((firstChild.Children[num].ObjectType == RTFObjectType.Group) && firstChild.Children[num].FirstChild.Key.Equals("fonttbl"))
                {
                    node3 = firstChild.Children[num];
                    break;
                }
                num++;
            }
            if (node3 != null)
            {
                int index = 0;
                string str = "";
                byte[] bytes = new byte[2];
                for (int i = 1; i < node3.Children.Count; i++)
                {
                    RTFTreeNode node4 = node3.Children[i];
                    foreach (RTFTreeNode node5 in node4.Children)
                    {
                        if (node5.ObjectType == RTFObjectType.Text)
                        {
                            if (str != "")
                            {
                                A_0.Add(str);
                                str = "";
                            }
                            A_0.Add(node5.Key.Substring(0, node5.Key.Length - 1));
                            index = 0;
                            continue;
                        }
                        if ((node5.ObjectType == RTFObjectType.Control) && node5.Key.Equals("'"))
                        {
                            bytes[index] = (byte) node5.Param;
                            if (index == 1)
                            {
                                str = str + Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                    }
                    if (str != "")
                    {
                        A_0.Add(str);
                    }
                }
            }
        }

        private string db(int A_0)
        {
            if (A_0 >= this.a.Count)
            {
                return "";
            }
            return (string) this.a[A_0];
        }

        private string db(string A_0)
        {
            return (this.db() + A_0 + this.da());
        }

        private string dc()
        {
            string str = "<html><body>";
            this.d = new FontStyle(this.db(0), HTMLTree.defaultFontSize, this.da(0), Colors.Transparent);
            bool flag = false;
            bool flag2 = false;
            int num = 0;
            RTFTreeNode node = null;
            while (!flag2 && (num < this.tree.RootNode.FirstChild.Children.Count))
            {
                node = this.tree.RootNode.FirstChild.Children[num];
                if (!flag)
                {
                    if (node.ObjectType == RTFObjectType.Group)
                    {
                        flag = true;
                    }
                }
                else if ((node.ObjectType != RTFObjectType.Group) && node.Key.Equals("pard"))
                {
                    flag2 = true;
                    break;
                }
                num++;
            }
            return (str + this.da(this.tree.RootNode.FirstChild, num) + "</body></html>");
        }

        private string dc(int A_0)
        {
            int num = 1;
            string str = "";
            while ((A_0 + 15) > Math.Pow(16.0, (double) (num - 1)))
            {
                int num2 = (int) (((double) A_0) % Math.Pow(16.0, (double) num));
                num2 = (int) (((double) num2) / Math.Pow(16.0, (double) (num - 1)));
                if (num2 <= 9)
                {
                    str = str + ((char) (num2 + 0x30));
                }
                else
                {
                    str = str + ((char) (num2 + 0x37));
                }
                A_0 -= num2;
                num++;
            }
            return this.da(str);
        }

        public string ParseRTF()
        {
            this.db(this.a);
            this.da(this.b);
            return this.dc();
        }
    }
}

