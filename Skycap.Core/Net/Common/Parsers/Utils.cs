namespace Skycap.Net.Common.Parsers
{
    using Skycap.Net.Common.MessageReaders;
    using System;
    using System.Text;

    public static class Utils
    {
        public static string ReadHeaders(IMessageReader reader)
        {
            StringBuilder builder = new StringBuilder();
            while (!reader.EndOfMessage)
            {
                byte[] bytes = reader.ReadLine();
                if (bytes == null)
                {
                    break;
                }
                string str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                if (str == "")
                {
                    break;
                }
                builder.Append(str + "\r\n");
            }
            if (builder.Length > 1)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            return builder.ToString();
        }
    }
}

