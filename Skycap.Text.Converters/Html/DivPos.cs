namespace Skycap.Text.Converters.Html
{
    using System;

    public class DivPos
    {
        private int a;
        private int b;
        private int c;
        private PositionType d;

        public DivPos()
        {
            this.c = -1;
            this.PosType = PositionType.Static;
            this.Left = 1;
            this.Top = 1;
        }

        public DivPos(PositionType type, int left, int top)
        {
            this.c = -1;
            this.PosType = type;
            this.Left = left;
            this.Top = top;
        }

        public int Left
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

        public PositionType PosType
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

        public int Top
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

        public int Width
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
    }
}

