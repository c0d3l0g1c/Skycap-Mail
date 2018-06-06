namespace Skycap.Net.Imap.Parsers
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class StringEncoding
    {
        private static string Decode(string source)
        {
            string str = source.Replace(',', '/');
            str = string.Format("+{0}-", str);
            byte[] bytes = new UTF8Encoding().GetBytes(str);
            return new UTF8Encoding().GetString(bytes, 0, bytes.Length);
        }

        public static string DecodeMailboxName(string name)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            EParseMode uSASCII = EParseMode.USASCII;
            EParseMode mode2 = EParseMode.USASCII;
            for (int i = 0; i < name.Length; i++)
            {
                switch (name[i])
                {
                    case '&':
                        mode2 = uSASCII;
                        uSASCII = EParseMode.Ampersand;
                        break;

                    case '-':
                        if (uSASCII == EParseMode.Base64)
                        {
                            builder.Append(Decode(builder2.ToString()));
                            mode2 = uSASCII;
                            uSASCII = EParseMode.USASCII;
                        }
                        else if (uSASCII == EParseMode.Ampersand)
                        {
                            if (mode2 == EParseMode.USASCII)
                            {
                                builder.Append('&');
                            }
                            else
                            {
                                builder2.Append('&');
                            }
                            uSASCII = mode2;
                        }
                        break;

                    default:
                        switch (uSASCII)
                        {
                            case EParseMode.Base64:
                                builder2.Append(name[i]);
                                break;

                            case EParseMode.USASCII:
                                builder.Append(name[i]);
                                break;
                        }
                        mode2 = uSASCII;
                        uSASCII = EParseMode.Base64;
                        new StringBuilder().Append(name[i]);
                        break;
                }
            }
            return builder.ToString();
        }

        public static string EncodeMailboxName(string name)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string[] strArray = name.Split(new char[] { '&' });
            string str = string.Empty;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (i > 0)
                {
                    str = str + "&-";
                }
                byte[] bytes = encoding.GetBytes(strArray[i]);
                str = str + new UTF8Encoding().GetString(bytes, 0, bytes.Length);
            }
            return Regex.Replace(str.Replace("+", "&"), "&(?<a>[^-/]*)(/)(?<b>[^&/]*)-", "&${a},${b}-");
        }

        private enum EParseMode
        {
            Base64,
            USASCII,
            Ampersand
        }
    }
}

