namespace Skycap.Net.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{DateTime} {Offset}")]
    public struct DateTimeOffset : IComparable
    {
        private System.DateTime _dateTime;
        private TimeSpan _offset;
        public static readonly TimeSpan MaxOffset;
        public static readonly TimeSpan MinOffset;
        public static readonly Skycap.Net.Common.DateTimeOffset MaxValue;
        public static readonly Skycap.Net.Common.DateTimeOffset MinValue;
        private System.DateTime dateTime
        {
            get
            {
                return this._dateTime;
            }
            set
            {
                this._dateTime = value;
            }
        }
        public System.DateTime DateTime
        {
            get
            {
                return this._dateTime;
            }
        }
        private TimeSpan offset
        {
            get
            {
                return this._offset;
            }
            set
            {
                this._offset = ValidateOffset(value);
            }
        }
        public TimeSpan Offset
        {
            get
            {
                return this.offset;
            }
        }
        private static TimeSpan ValidateOffset(TimeSpan value)
        {
            if ((MaxOffset < value) || (value < MinOffset))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            return value;
        }

        static DateTimeOffset()
        {
            MaxOffset = new TimeSpan(14, 0, 0);
            MinOffset = new TimeSpan(-12, 0, 0);
            MinValue = new Skycap.Net.Common.DateTimeOffset(System.DateTime.MinValue, new TimeSpan(0, 0, 0));
            MaxValue = new Skycap.Net.Common.DateTimeOffset(System.DateTime.MaxValue, new TimeSpan(0, 0, 0));
        }

        public DateTimeOffset(System.DateTime dateTime)
        {
            this._dateTime = dateTime;
            this._offset = new TimeSpan();
        }

        public DateTimeOffset(System.DateTime dateTime, TimeSpan offset)
        {
            this._dateTime = dateTime;
            this._offset = ValidateOffset(offset);
        }

        public System.DateTime UtcDateTime
        {
            get
            {
                return (System.DateTime.SpecifyKind(this._dateTime, DateTimeKind.Utc) - this._offset);
            }
        }
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (!(obj is Skycap.Net.Common.DateTimeOffset))
            {
                throw new ArgumentException("obj is not DateTimeOffset");
            }
            Skycap.Net.Common.DateTimeOffset offset = (Skycap.Net.Common.DateTimeOffset) obj;
            System.DateTime utcDateTime = offset.UtcDateTime;
            System.DateTime time2 = this.UtcDateTime;
            if (time2 > utcDateTime)
            {
                return 1;
            }
            if (time2 < utcDateTime)
            {
                return -1;
            }
            return 0;
        }
    }
}

