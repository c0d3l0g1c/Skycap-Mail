namespace Skycap.Text.Converters.Html
{
    using System;
    using Windows.UI;

    public class RectStyle
    {
        private BorderStyle a = new BorderStyle();
        private BorderStyle b = new BorderStyle();
        private BorderStyle c = new BorderStyle();
        private BorderStyle d = new BorderStyle();
        private Color e;
        private bool f;
        private Alignment g;
        private int h = -1;
        private int i = -1;
        private LengthType j;
        private LengthType k;
        private int l = -1;

        public RectStyle()
        {
            this.i = -1;
            this.SetMargin(3);
        }

        public void AssignTo(RectStyle rect)
        {
            rect.f = this.f;
            rect.e = this.e;
            rect.g = this.g;
            rect.i = this.i;
            this.a.AssignTo(rect.a);
            this.c.AssignTo(rect.c);
            this.b.AssignTo(rect.b);
            this.d.AssignTo(rect.d);
        }

        public void SetBorderColor(Color color)
        {
            this.TopBorder.BorderColor = color;
            this.LeftBorder.BorderColor = color;
            this.BottomBorder.BorderColor = color;
            this.RightBorder.BorderColor = color;
        }

        public void SetBorderType(BorderType type)
        {
            this.TopBorder.Type = type;
            this.LeftBorder.Type = type;
            this.BottomBorder.Type = type;
            this.RightBorder.Type = type;
        }

        public void SetBorderWidth(int width)
        {
            if (width <= 0)
            {
                this.SetBorderType(BorderType.NONE);
            }
            else
            {
                this.TopBorder.Width = width;
                this.LeftBorder.Width = width;
                this.BottomBorder.Width = width;
                this.RightBorder.Width = width;
            }
        }

        public void SetMargin(int margin)
        {
            this.TopBorder.Margin = margin;
            this.LeftBorder.Margin = margin;
            this.BottomBorder.Margin = margin;
            this.RightBorder.Margin = margin;
        }

        public Alignment Align
        {
            get
            {
                return this.g;
            }
            set
            {
                this.g = value;
            }
        }

        public Color BGColor
        {
            get
            {
                return this.e;
            }
            set
            {
                this.e = value;
                this.f = true;
            }
        }

        public BorderStyle BottomBorder
        {
            get
            {
                return this.d;
            }
        }

        public bool HasBgColor
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

        public int Height
        {
            get
            {
                return this.l;
            }
            set
            {
                this.l = value;
            }
        }

        public LengthType HeightType
        {
            get
            {
                return this.k;
            }
            set
            {
                this.k = value;
            }
        }

        public BorderStyle LeftBorder
        {
            get
            {
                return this.a;
            }
        }

        public int OrginWidth
        {
            get
            {
                return this.i;
            }
            set
            {
                this.i = value;
            }
        }

        public BorderStyle RightBorder
        {
            get
            {
                return this.c;
            }
        }

        public BorderStyle TopBorder
        {
            get
            {
                return this.b;
            }
        }

        public int Width
        {
            get
            {
                return this.h;
            }
            set
            {
                this.h = value;
            }
        }

        public LengthType WidthType
        {
            get
            {
                return this.j;
            }
            set
            {
                this.j = value;
            }
        }
    }
}

