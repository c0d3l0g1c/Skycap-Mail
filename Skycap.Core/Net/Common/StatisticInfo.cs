using System;

using Skycap.Net.Imap;
using Skycap.Net.Imap.Collections;

namespace Skycap.Net.Common
{
    public class StatisticInfo
    {
        public StatisticInfo(string uniqueNumber, uint serialNumber, uint messageSize, string flags)
        {
            UniqueNumber = uniqueNumber;
            SerialNumber = serialNumber;
            MessageSize = messageSize;
            Flags = ParseFlags(flags);
        }

        public string UniqueNumber
        {
            get;
            private set;
        }

        public uint SerialNumber
        {
            get;
            private set;
        }

        public uint MessageSize
        {
            get;
            private set;
        }

        public virtual MessageFlagCollection Flags
        {
            get;
            private set;
        }

        public MessageFlagCollection ParseFlags(string flags)
        {
            MessageFlagCollection messageFlagCollection = new MessageFlagCollection();
            if (!string.IsNullOrEmpty(flags) && flags.Contains(@"\"))
            {
                string[] flagNames = flags.Split(new char[] { '\\', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string flag in flagNames)
                    messageFlagCollection.Add(new MessageFlag(flag));
            }
            return messageFlagCollection;
        }
    }
}
