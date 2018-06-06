namespace Skycap.Text.Converters.Rtf
{
    using System;

    public class RTFObject
    {
        private RTFObjectType a;
        private string b = "";
        private bool c;
        private int d;

        public bool HasParam
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

        public string Key
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

        public RTFObjectType ObjectType
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

        public int Param
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
    }
}

