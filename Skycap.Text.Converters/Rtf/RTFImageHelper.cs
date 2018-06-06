namespace Skycap.Text.Converters.Rtf
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Skycap.Text.Converters.Html;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml.Media.Imaging;

    public class RTFImageHelper
    {
        public async static Task<string> LoadFromHtml(HTMLNode imgNode, string baseUrl)
        {
            Param param = imgNode.Param.ByName("src");
            string str = "";
            if (param == null)
            {
                return str;
            }
            string url = param.Value.Trim().ToLower();
            if ((url == "#") || url.Equals(string.Empty))
            {
                return "";
            }
            if ((url.StartsWith("http://") || url.StartsWith("ftp://")) || url.StartsWith("file://"))
            {
                url = param.Value.Trim();
            }
            else
            {
                url = baseUrl + param.Value.Trim();
            }
            ImageStyle style = new ImageStyle(url);
            using (Stream stream = await NetHelper.Get(url))
            {
                int width = 0;
                int height = 0;
                RandomStream rs = new RandomStream(stream);
                BitmapImage image = new BitmapImage();
                {
                    image.SetSource(rs);
                    width = image.PixelWidth;
                    height = image.PixelHeight;
                    Param param2 = imgNode.Param.ByName("width");
                    if (param2 != null)
                    {
                        style.Width = HTMLTree.FontSizeFromHTML(param2.Value.Trim());
                    }
                    Param param3 = imgNode.Param.ByName("height");
                    if (param3 != null)
                    {
                        style.Height = HTMLTree.FontSizeFromHTML(param3.Value.Trim());
                    }
                    if ((style.Width != -1) && (style.Height != -1))
                    {
                        width = style.Width;
                        height = style.Height;
                    }
                    else if (style.Width != -1)
                    {
                        height = (width * style.Height) / style.Width;
                        width = style.Width;
                    }
                    else if (style.Height != -1)
                    {
                        width = (height * style.Width) / style.Height;
                        height = style.Height;
                    }
                }
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("{{\\pict\\wmetafile8\\picwgoal{0}\\pichgoal{1}\n", width * 20, height * 20);
                byte[] buffer = new byte[40];
                stream.Position = 0L;
                int num3 = 0;
                while ((num3 = stream.Read(buffer, 0, 40)) > 0)
                {
                    for (int i = 0; i < num3; i++)
                    {
                        builder.Append(buffer[i].ToString("x").PadLeft(2, '0'));
                    }
                    builder.Append("\n");
                }
                builder.Append("}");
                return builder.ToString();
            }
        }

        public static string LoadFromRtf(string rtf, string tempFolder)
        {
            return "";
        }
    }

    public static class MicrosoftStreamExtensions
    {
        public static IRandomAccessStream AsRandomAccessStream(this Stream stream)
        {
            return new RandomStream(stream);
        }
    }
    class RandomStream : IRandomAccessStream
    {
        Stream internstream;
        public RandomStream(Stream underlyingstream)
        {
            internstream = underlyingstream;
        }
        public IInputStream GetInputStreamAt(ulong position)
        {
            //THANKS Microsoft! This is GREATLY appreciated!
            internstream.Position = (long)position;
            return internstream.AsInputStream();
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            internstream.Position = (long)position;
            return internstream.AsOutputStream();
        }

        public ulong Size
        {
            get
            {
                return (ulong)internstream.Length;
            }
            set
            {
                internstream.SetLength((long)value);
            }
        }

        public bool CanRead
        {
            get { return internstream.CanRead; }
        }

        public bool CanWrite
        {
            get { return internstream.CanWrite; }
        }

        public IRandomAccessStream CloneStream()
        {
            return new RandomStream(internstream);
        }

        public ulong Position
        {
            get { return (ulong)internstream.Position; }
        }

        public void Seek(ulong position)
        {
            internstream.Seek((long)position, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            internstream.Dispose();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return GetInputStreamAt(0).ReadAsync(buffer, count, options);
        }

        public Windows.Foundation.IAsyncOperation<bool> FlushAsync()
        {
            return GetOutputStreamAt(0).FlushAsync();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            return GetOutputStreamAt(0).WriteAsync(buffer);
        }
    }
}

