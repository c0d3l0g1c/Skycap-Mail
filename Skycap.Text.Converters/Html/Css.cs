namespace Skycap.Text.Converters.Html
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Css
    {
        public string Name;
        public string Value;
        public CssType Type;
        public string For;
    }
}

