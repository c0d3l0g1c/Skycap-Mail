using System;
using System.Collections.Generic;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Defines a method to compare mail headers.
    /// </summary>
    public class MailHeaderComparer : IComparer<MailHeader>
    {
        /// <summary>
        /// Compares two mail headers and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first mail header to compare.</param>
        /// <param name="y">The second mail header to compare.</param>
        /// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.Value Meaning Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.</returns>
        public int Compare(MailHeader x, MailHeader y)
        {
            int date = x.Date.CompareTo(y.Date) * -1;
            if (date == 0)
                return x.Uid.PadRight(10, '0').CompareTo(y.Uid.PadRight(10, '0')) * -1;
            else
                return date;
        }
    }
}
