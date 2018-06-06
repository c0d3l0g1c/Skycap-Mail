namespace Skycap.Text.Converters.Html
{
    using System;

    public class TableCol
    {
        private int a = 1;
        private int b = 1;
        private RectStyle c = new RectStyle();
        private bool d;

        public int ColSpan
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

        public bool IsUsed
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

        public RectStyle Rect
        {
            get
            {
                return this.c;
            }
        }

        public int RowSpan
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
    }
}

