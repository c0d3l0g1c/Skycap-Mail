namespace Skycap.Net.Common
{
    using System;
    using System.Text;

    public static class BoundaryChecker
    {
        public static EBoundaryType CheckBoundary(byte[] line, string boundary)
        {
            if (boundary != null)
            {
                string str = Encoding.UTF8.GetString(line, 0, line.Length);
                if (str == ("--" + boundary))
                {
                    return EBoundaryType.Intermediate;
                }
                if (str == ("--" + boundary + "--"))
                {
                    return EBoundaryType.Final;
                }
            }
            return EBoundaryType.NotBoundary;
        }
    }
}

