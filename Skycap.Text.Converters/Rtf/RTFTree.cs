namespace Skycap.Text.Converters.Rtf
{
    using System;
    using System.IO;
    using System.Text;

    public class RTFTree
    {
        private RTFTreeNode a = new RTFTreeNode(RTFObjectType.Root);
        private TextReader b;
        private RTFContainer c;
        private RTFObject d;
        private int e = 0;

        private int da()
        {
            int num = 0;
            RTFTreeNode a = this.a;
            RTFTreeNode node = null;
            this.d = this.c.nextObject();
            while (this.d.ObjectType != RTFObjectType.EOF)
            {
                switch (this.d.ObjectType)
                {
                    case RTFObjectType.KeyWord:
                    case RTFObjectType.Control:
                    case RTFObjectType.Text:
                        node = new RTFTreeNode(this.d);
                        a.Append(node);
                        break;

                    case RTFObjectType.GroupStart:
                        node = new RTFTreeNode(RTFObjectType.Group);
                        a.Append(node);
                        a = node;
                        this.e++;
                        break;

                    case RTFObjectType.GroupEnd:
                        a = a.Parent;
                        this.e--;
                        break;

                    default:
                        num = -1;
                        break;
                }
                this.d = this.c.nextObject();
            }
            if (this.e != 0)
            {
                num = -1;
            }
            return num;
        }

        private string da(string A_0)
        {
            MemoryStream a = new MemoryStream(Encoding.UTF8.GetBytes(A_0));
            using (StreamReader reader = new StreamReader(a, Encoding.UTF8, true))
            {
                return reader.ReadToEnd();
            }
        }

        public int LoadRTFFile(string path)
        {
            this.b = new StringReader(this.da(path));
            this.c = new RTFContainer(this.b);
            return this.da();
        }

        public int LoadRtfText(string text)
        {
            this.b = new StringReader(text);
            this.c = new RTFContainer(this.b);
            return this.da();
        }

        public RTFTreeNode RootNode
        {
            get
            {
                return this.a;
            }
        }
    }
}

