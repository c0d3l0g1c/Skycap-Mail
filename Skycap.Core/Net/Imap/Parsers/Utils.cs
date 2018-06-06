namespace Skycap.Net.Imap.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class Utils
    {
        private const string _exIncorrectFormat = "The source string was not in a correct format";
        private static readonly UTF8Encoding DefaultEncoding = new UTF8Encoding();

        public static List<byte[]> ExtractParams(byte[] source)
        {
            int num = 0;
            EFetchResponseParserState state = EFetchResponseParserState.Base;
            int start = 0;
            int end = 0;
            uint num4 = 0;
            List<byte[]> list = new List<byte[]>();
            uint num5 = 0;
            for (int i = 0; i < source.Length; i++)
            {
                EFetchResponseParserState inParanthesis = state;
                switch (state)
                {
                    case EFetchResponseParserState.Base:
                        if (source[i] != 40)
                        {
                            break;
                        }
                        num++;
                        inParanthesis = EFetchResponseParserState.InParanthesis;
                        start = i + 1;
                        goto Label_0197;

                    case EFetchResponseParserState.InQuotes:
                        if (source[i] == 0x5c)
                        {
                            inParanthesis = EFetchResponseParserState.AfterSlash;
                        }
                        if (source[i] == 0x22)
                        {
                            inParanthesis = EFetchResponseParserState.Base;
                            end = i;
                        }
                        goto Label_0197;

                    case EFetchResponseParserState.InParanthesis:
                        if (source[i] == 40)
                        {
                            num++;
                        }
                        if (source[i] == 0x29)
                        {
                            num--;
                            if (num == 0)
                            {
                                inParanthesis = EFetchResponseParserState.Base;
                                end = i;
                            }
                        }
                        goto Label_0197;

                    case EFetchResponseParserState.InWord:
                        if (source[i] != 0x20)
                        {
                            goto Label_0120;
                        }
                        inParanthesis = EFetchResponseParserState.Base;
                        end = i;
                        goto Label_0197;

                    case EFetchResponseParserState.InMultilineHeader:
                        if (source[i] == 0x7d)
                        {
                            inParanthesis = EFetchResponseParserState.AfterMultilineHeader;
                            end = i;
                            byte[] bytes = ExtractSubarray(source, start, end);
                            num4 = uint.Parse(DefaultEncoding.GetString(bytes, 0, bytes.Length));
                        }
                        goto Label_0197;

                    case EFetchResponseParserState.InMultilineBlock:
                        num5++;
                        if (num5 >= num4)
                        {
                            inParanthesis = EFetchResponseParserState.Base;
                            end = i + 1;
                        }
                        goto Label_0197;

                    case EFetchResponseParserState.AfterMultilineHeader:
                        if (source[i] == 10)
                        {
                            inParanthesis = EFetchResponseParserState.InMultilineBlock;
                            start = i + 1;
                        }
                        goto Label_0197;

                    case EFetchResponseParserState.AfterSlash:
                        inParanthesis = EFetchResponseParserState.InQuotes;
                        goto Label_0197;

                    default:
                        goto Label_0197;
                }
                if (source[i] == 0x22)
                {
                    inParanthesis = EFetchResponseParserState.InQuotes;
                    start = i + 1;
                }
                else if (source[i] == 0x7b)
                {
                    inParanthesis = EFetchResponseParserState.InMultilineHeader;
                    start = i + 1;
                }
                else
                {
                    if ((source[i] == 0x7d) || (source[i] == 0x29))
                    {
                        throw new FormatException("The source string was not in a correct format");
                    }
                    if (source[i] != 0x20)
                    {
                        inParanthesis = EFetchResponseParserState.InWord;
                        start = i;
                    }
                }
                goto Label_0197;
            Label_0120:
                if (((source[i] == 0x22) || (source[i] == 40)) || (source[i] == 0x29))
                {
                    throw new FormatException("The source string was not in a correct format");
                }
            Label_0197:
                if ((state != inParanthesis) && (inParanthesis == EFetchResponseParserState.Base))
                {
                    list.Add(ExtractSubarray(source, start, end));
                }
                state = inParanthesis;
            }
            if (state == EFetchResponseParserState.InWord)
            {
                list.Add(ExtractSubarray(source, start, source.Length));
                return list;
            }
            if (state != EFetchResponseParserState.Base)
            {
                throw new FormatException("The source string was not in a correct format");
            }
            return list;
        }

        private static byte[] ExtractSubarray(byte[] source, int start, int end)
        {
            byte[] destinationArray = new byte[end - start];
            Array.Copy(source, start, destinationArray, 0, end - start);
            return destinationArray;
        }

        public static byte[] Unparenthesis(byte[] source)
        {
            if (((source.Length > 0) && (source[0] == 40)) && (source[source.Length - 1] == 0x29))
            {
                return ExtractSubarray(source, 1, source.Length - 1);
            }
            return source;
        }

        public static string Unquote(string source)
        {
            return source.Replace("\"", "");
        }
    }
}

