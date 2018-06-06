namespace Skycap.Net.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class StreamExtensions
    {
        public const int BufferSize = 0x400;

        public static byte[] ReadToEnd(Stream stream)
        {
            int num;
            List<byte> list = new List<byte>();
            byte[] buffer = new byte[0x400];
            do
            {
                num = stream.Read(buffer, 0, buffer.Length);
                if (num == buffer.Length)
                {
                    list.AddRange(buffer);
                }
                else
                {
                    for (int i = 0; i < num; i++)
                    {
                        list.Add(buffer[i]);
                    }
                }
            }
            while (num == buffer.Length);
            return list.ToArray();
        }

        public static byte[] ReadToEndLine(Stream stream)
        {
            return ReadToEndLine(stream, uint.MaxValue);
        }

        public static byte[] ReadToEndLine(Stream stream, uint maxBytesToRead)
        {
            List<byte> list = ReceiveLine(stream, maxBytesToRead);
            if (((list.Count > 1) && (list[list.Count - 2] == 13)) && (list[list.Count - 1] == 10))
            {
                list.RemoveRange(list.Count - 2, 2);
            }
            return list.ToArray();
        }

        private static List<byte> ReceiveLine(Stream stream, uint maxBytesToRead)
        {
            ELineReaderState plain = ELineReaderState.Plain;
            List<byte> list = new List<byte>();
            for (uint i = 0; (plain != ELineReaderState.Got0D0A) && (maxBytesToRead > i); i++)
            {
                int num2 = stream.ReadByte();
                if (num2 == -1)
                {
                    return list;
                }
                byte item = Convert.ToByte(num2);
                switch (plain)
                {
                    case ELineReaderState.Plain:
                        if (item == 13)
                        {
                            plain = ELineReaderState.Got0D;
                        }
                        list.Add(item);
                        break;

                    case ELineReaderState.Got0D:
                        plain = (item == 10) ? ELineReaderState.Got0D0A : ELineReaderState.Plain;
                        list.Add(item);
                        break;
                }
            }
            return list;
        }

        private enum ELineReaderState
        {
            Plain,
            Got0D,
            Got0D0A
        }
    }
}

