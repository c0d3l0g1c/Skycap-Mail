namespace Skycap.Net.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class MailMessageRFCDecoder
    {
        private static byte _toByte;
        public static readonly Regex regArgument = new Regex(@"^([^:]+): *([^\r;]+)", RO);
        private static readonly Regex regBase64 = new Regex("^[a-zA-Z0-9+/=]+$", RO);
        public static readonly Regex regDifArgumentValue = new Regex("^\"([^\"]+)\"$", RO);
        private static readonly Regex regSubjectItem = new Regex("=\\?([^?]+)\\?([^?]+)\\?([^?]+)\\?=([\r\n\t]+)?", RO);
        private static readonly Regex regUnfold = new Regex(@"\r\n[ \t]+", RO);
        public static readonly RegexOptions RO = (RegexOptions.IgnoreCase);

        public static byte[] DecodeFromBase64(byte[] source)
        {
            byte[] buffer;
            string s = Encoding.UTF8.GetString(source, 0, source.Length);
            try
            {
                buffer = Convert.FromBase64String(s);
            }
            catch (FormatException)
            {
                return source;
            }
            return buffer;
        }

        public static byte[] DecodeFromQuotedPrintable(byte[] buf)
        {
            List<byte> list = new List<byte>();
            EQuotedPrintableDecoderState plain = EQuotedPrintableDecoderState.Plain;
            char[] chArray = new char[2];
            foreach (byte num in buf)
            {
                switch (plain)
                {
                    case EQuotedPrintableDecoderState.Plain:
                        if (num != 0x3d)
                        {
                            break;
                        }
                        plain = EQuotedPrintableDecoderState.GotEqualSign;
                        goto Label_007E;

                    case EQuotedPrintableDecoderState.GotFirstQuoted:
                        chArray[1] = (char) num;
                        try
                        {
                            _toByte = Convert.ToByte(new string(chArray), 0x10);
                        }
                        catch (ArgumentException)
                        {
                            _toByte = 0x3f;
                        }
                        list.Add(_toByte);
                        plain = EQuotedPrintableDecoderState.Plain;
                        goto Label_007E;

                    case EQuotedPrintableDecoderState.GotEqualSign:
                        chArray[0] = (char) num;
                        plain = EQuotedPrintableDecoderState.GotFirstQuoted;
                        goto Label_007E;

                    default:
                        goto Label_007E;
                }
                list.Add(num);
            Label_007E:;
            }
            return list.ToArray();
        }

        public static string GetStringFromBase64(byte[] value, string encoding)
        {
            return GetStringFromBase64(Encoding.UTF8.GetString(value, 0, value.Length).Replace("\r\n", ""), encoding);
        }

        public static string GetStringFromBase64(string value, string encoding)
        {
            Encoding encoding2;
            byte[] buffer;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (!regBase64.IsMatch(value))
            {
                return value;
            }
            try
            {
                encoding2 = Encoding.GetEncoding(encoding);
            }
            catch (ArgumentException)
            {
                return value;
            }
            try
            {
                buffer = Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                return value;
            }
            return encoding2.GetString(buffer, 0, buffer.Length);
        }

        public static string GetStringFromQuotedPrintable(byte[] text, string encoding)
        {
            return GetStringFromQuotedPrintable(Encoding.UTF8.GetString(text, 0, text.Length), encoding);
        }

        public static string GetStringFromQuotedPrintable(string text, string encoding)
        {
            Encoding encoding2;
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            try
            {
                encoding2 = Encoding.GetEncoding(encoding);
            }
            catch (ArgumentException)
            {
                return text;
            }
            StringBuilder builder = new StringBuilder(text);
            builder.Replace("=\r\n", "");
            List<byte> list = new List<byte>();
            for (int i = 0; i < builder.Length; i++)
            {
                if (builder[i] != '=')
                {
                    list.Add((byte) builder[i]);
                }
                else
                {
                    try
                    {
                        byte item = byte.Parse(builder[++i] + builder[++i].ToString(), NumberStyles.AllowHexSpecifier);
                        list.Add(item);
                    }
                    catch
                    {
                        i -= 2;
                    }
                }
            }
            return encoding2.GetString(list.ToArray(), 0, list.Count);
        }

        public static string ParseBase64Item(string item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            Match match = regSubjectItem.Match(item);
            if ((match.Success && (match.Groups.Count > 2)) && (match.Groups[2].Value.ToUpper() == "B"))
            {
                return GetStringFromBase64(match.Groups[3].Value, match.Groups[1].Value);
            }
            if ((match.Success && (match.Groups.Count > 2)) && (match.Groups[2].Value.ToUpper() == "Q"))
            {
                return GetStringFromQuotedPrintable(match.Groups[3].Value.Replace('_', ' '), match.Groups[1].Value);
            }
            return "";
        }

        public static string Unfolding(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            text = text.Replace("=\r\n", " ");
            text = regUnfold.Replace(text, " ");
            return text;
        }

        public static StringBuilder Unfolding(StringBuilder text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            text.Replace("=\r\n", " ");
            text.Replace("\r\n\t", " ");
            text.Replace("\r\n ", " ");
            return text;
        }

        private enum EQuotedPrintableDecoderState
        {
            Plain,
            GotFirstQuoted,
            GotEqualSign
        }
    }
}

