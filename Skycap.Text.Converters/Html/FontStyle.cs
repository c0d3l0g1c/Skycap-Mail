namespace Skycap.Text.Converters.Html
{
    using System;
    using System.Runtime.InteropServices;
    using Windows.UI;

    [StructLayout(LayoutKind.Sequential)]
    public struct FontStyle
    {
        public string FontName;
        public uint FontSize;
        public Color FontColor;
        public bool IsUnderLine;
        public bool IsItalic;
        public bool IsBold;
        public Alignment Align;
        public Color BgColor;
        public string Link;
        public RaiseType Raise;
        public FontStyle(string fontName, uint fontSize, Color fontColor, bool underline, bool italic, bool bold, Alignment align, Color bgColor)
        {
            this.FontName = fontName;
            this.FontSize = fontSize;
            this.FontColor = fontColor;
            this.IsUnderLine = underline;
            this.IsItalic = italic;
            this.IsBold = bold;
            this.Align = align;
            this.BgColor = bgColor;
            this.Link = "";
            this.Raise = RaiseType.Normal;
        }

        public FontStyle(FontStyle style)
        {
            this.FontName = style.FontName;
            this.FontSize = style.FontSize;
            this.FontColor = style.FontColor;
            this.IsBold = false;
            this.IsItalic = false;
            this.IsUnderLine = false;
            this.Align = Alignment.Default;
            this.BgColor = style.BgColor;
            this.Link = "";
            this.Raise = RaiseType.Normal;
        }

        public FontStyle(string fontName, uint fontSize, Color fontColor, Color bgColor) : this(fontName, fontSize, fontColor, false, false, false, Alignment.Default, bgColor)
        {
        }
    }
}

