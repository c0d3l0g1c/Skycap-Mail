namespace Skycap.Text.Converters.Html
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageStyle
    {
        public string Url;
        public int Width;
        public int Height;
        public FloatType Float;
        public FileType Type;
        public ImageStyle(string url, int width, int height, FloatType floattype)
        {
            this.Url = url;
            this.Width = width;
            this.Height = height;
            this.Float = floattype;
            string str = this.Url.Substring(0, 5).ToLower();
            if ((str.StartsWith("http") || str.StartsWith("https")) || (str.StartsWith("ftp") || str.StartsWith("file")))
            {
                this.Type = FileType.URL;
            }
            else
            {
                this.Type = FileType.File;
            }
        }

        public ImageStyle(string url, int width, int height) : this(url, width, height, FloatType.None)
        {
        }

        public ImageStyle(string url) : this(url, -1, -1, FloatType.None)
        {
        }
    }
}

