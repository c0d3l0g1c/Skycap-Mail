namespace Skycap.Text.Converters.Html
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public class NetHelper
    {
        private static Stream a(Stream A_0)
        {
            MemoryStream stream = new MemoryStream();
            byte[] buffer = new byte[0x800];
            int count = 0;
            while ((count = A_0.Read(buffer, 0, 0x800)) > 0)
            {
                stream.Write(buffer, 0, count);
            }
            return stream;
        }

        public async static Task<Stream> Get(string url)
        {
            WebResponse wr = await HttpWebRequest.Create(url).GetResponseAsync();
            return a(wr.GetResponseStream());
        }

        public async static Task<Stream> Get(Uri url)
        {
            WebResponse wr = await HttpWebRequest.Create(url).GetResponseAsync();
            return a(wr.GetResponseStream());
        }
    }
}

