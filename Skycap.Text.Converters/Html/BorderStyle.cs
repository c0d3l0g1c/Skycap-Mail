namespace Skycap.Text.Converters.Html
{
    using System;
    using Windows.UI;

    public class BorderStyle
    {
        private int a;
        private BorderType b;
        private Color c;
        private int d;

        public BorderStyle()
        {
            this.a = 0;
            this.b = BorderType.NONE;
            this.c = Colors.Black;
            this.d = 3;
        }

        public BorderStyle(int width, BorderType type, Color color) : this(width, type, color, 3)
        {
        }

        public BorderStyle(int width, BorderType type, Color color, int margin)
        {
            this.Width = width;
            this.c = color;
            this.Type = type;
            this.d = margin;
        }

        public void AssignTo(BorderStyle style)
        {
            style.BorderColor = this.BorderColor;
            style.a = this.a;
            style.b = this.b;
            style.Margin = style.Margin;
        }

        public Color BorderColor
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

        public int Margin
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

        public BorderType Type
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
                return this.a;
            }
            set
            {
                this.a = value;
                this.b = BorderType.SOLID;
            }
        }
    }
}

