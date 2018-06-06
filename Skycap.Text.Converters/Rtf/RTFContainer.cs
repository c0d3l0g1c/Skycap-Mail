namespace Skycap.Text.Converters.Rtf
{
    using System;
    using System.IO;

    public class RTFContainer
    {
        private const int a = -1;
        private TextReader b;

        public RTFContainer(TextReader rtffile)
        {
            this.b = rtffile;
        }

        private void da(RTFObject A_0)
        {
            string str = "";
            string str2 = "";
            int num = 0;
            bool flag = false;
            int num2 = this.b.Peek();
            if (!this.db(num2))
            {
                this.b.Read();
                A_0.ObjectType = RTFObjectType.Control;
                A_0.Key = ((char) num2).ToString();
                if (A_0.Key == "'")
                {
                    string str3 = "";
                    str3 = str3 + ((char) this.b.Read()) + ((char) this.b.Read());
                    A_0.HasParam = true;
                    A_0.Param = Convert.ToInt32(str3, 0x10);
                }
            }
            else
            {
                num2 = this.b.Peek();
                while (this.db(num2))
                {
                    this.b.Read();
                    str = str + ((char) num2);
                    num2 = this.b.Peek();
                }
                A_0.ObjectType = RTFObjectType.KeyWord;
                A_0.Key = str;
                if (this.da(num2) || (num2 == 0x2d))
                {
                    A_0.HasParam = true;
                    if (num2 == 0x2d)
                    {
                        flag = true;
                        this.b.Read();
                    }
                    num2 = this.b.Peek();
                    while (this.da(num2))
                    {
                        this.b.Read();
                        str2 = str2 + ((char) num2);
                        num2 = this.b.Peek();
                    }
                    num = Convert.ToInt32(str2);
                    if (flag)
                    {
                        num = -num;
                    }
                    A_0.Param = num;
                }
                if (num2 == 0x20)
                {
                    this.b.Read();
                }
            }
        }

        private bool da(int A_0)
        {
            return ((A_0 >= 0x30) && (A_0 <= 0x39));
        }

        private void da(int A_0, RTFObject A_1)
        {
            int num = A_0;
            string str = ((char) num).ToString();
            for (num = this.b.Peek(); ((num != 0x5c) && (num != 0x7d)) && ((num != 0x7b) && (num != -1)); num = this.b.Peek())
            {
                this.b.Read();
                str = str + ((char) num);
            }
            A_1.Key = str;
        }

        private bool db(int A_0)
        {
            if (!this.d(A_0))
            {
                return this.c(A_0);
            }
            return true;
        }

        private bool c(int A_0)
        {
            return ((A_0 >= 0x41) && (A_0 <= 90));
        }

        private bool d(int A_0)
        {
            return ((A_0 >= 0x61) && (A_0 <= 0x7a));
        }

        public RTFObject nextObject()
        {
            RTFObject obj2 = new RTFObject();
            int num = this.b.Read();
            while (((num == 13) || (num == 10)) || ((num == 9) || (num == 0)))
            {
                num = this.b.Read();
            }
            switch (num)
            {
                case 0x7b:
                    obj2.ObjectType = RTFObjectType.GroupStart;
                    return obj2;

                case 0x7d:
                    obj2.ObjectType = RTFObjectType.GroupEnd;
                    return obj2;

                case 0x5c:
                    this.da(obj2);
                    return obj2;

                case -1:
                    obj2.ObjectType = RTFObjectType.EOF;
                    return obj2;
            }
            obj2.ObjectType = RTFObjectType.Text;
            this.da(num, obj2);
            return obj2;
        }
    }
}

