namespace Skycap.Net.Common.Extensions
{
    using System;
    using System.Text;

    public static class StringBuilderExtensions
    {
        public static bool EndsWith(StringBuilder builder, string value, bool ignoreCase)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int length = value.Length;
            if ((length > 0) && (builder.Length == 0))
            {
                return false;
            }
            if ((length > 0) && (string.Compare(builder.ToString(builder.Length - length, length), value, StringComparison.CurrentCultureIgnoreCase) != 0))
            {
                return false;
            }
            return true;
        }

        public static bool StartWith(StringBuilder builder, string value, bool ignoreCase)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int length = value.Length;
            if ((length > 0) && (builder.Length == 0))
            {
                return false;
            }
            if ((length > 0) && (string.Compare(builder.ToString(0, length), value, StringComparison.CurrentCultureIgnoreCase) != 0))
            {
                return false;
            }
            return true;
        }

        public static StringBuilder Trim(StringBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (builder.Length > 0)
            {
                int length = 0;
                int num2 = 0;
                while (char.IsWhiteSpace(builder[length]))
                {
                    length++;
                }
                while (char.IsWhiteSpace(builder[(builder.Length - 1) - num2]))
                {
                    num2++;
                }
                if (length > 0)
                {
                    builder.Remove(0, length);
                }
                if (num2 > 0)
                {
                    builder.Remove(builder.Length - num2, num2);
                }
            }
            return builder;
        }
    }
}

