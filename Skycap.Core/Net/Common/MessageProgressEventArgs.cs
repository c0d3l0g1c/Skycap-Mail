using System.Collections.Generic;

using Skycap.IO;

using Windows.Storage;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents the DownloadingMessage event data.
    /// </summary>
    public class MessageProgressEventArgs : MessageEventArgs
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.DownloadingMessageEventArgs class.
        /// </summary>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="statisticInfo">The statistic info.</param>
        /// <param name="totalMessageCount">The total message count.</param>
        /// <param name="totalMessageSize">The total message size.</param>
        /// <param name="currentMessageCount">The total message count.</param>
        /// <param name="currentMessageSize">The total message size.</param>
        /// <param name="remainingMessageCount">The remaining message count.</param>
        /// <param name="remainingMessageSize">The remaining message size.</param>
        public MessageProgressEventArgs(Mailbox mailbox, StatisticInfo statisticInfo, uint totalMessageCount, uint totalMessageSize, uint currentMessageCount, uint currentMessageSize, uint remainingMessageCount, uint remainingMessageSize)
            : this(null, null, mailbox, statisticInfo, totalMessageCount, totalMessageSize, currentMessageCount, currentMessageSize, remainingMessageCount, remainingMessageSize)
        {

        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Net.Common.DownloadingMessageEventArgs class.
        /// </summary>
        /// <param name="uidsFile">The uids file.</param>
        /// <param name="storedUids">The stored uids.</param>
        /// <param name="mailbox">The mailbox.</param>
        /// <param name="statisticInfo">The statistic info.</param>
        /// <param name="totalMessageCount">The total message count.</param>
        /// <param name="totalMessageSize">The total message size.</param>
        /// <param name="currentMessageCount">The total message count.</param>
        /// <param name="currentMessageSize">The total message size.</param>
        /// <param name="remainingMessageCount">The remaining message count.</param>
        /// <param name="remainingMessageSize">The remaining message size.</param>
        public MessageProgressEventArgs(StorageFile uidsFile, IList<string> storedUids, Mailbox mailbox, StatisticInfo statisticInfo, uint totalMessageCount, uint totalMessageSize, uint currentMessageCount, uint currentMessageSize, uint remainingMessageCount, uint remainingMessageSize)
            : base(mailbox, statisticInfo.UniqueNumber, null)
        { 
            // Initialise local variables
            UidsFile = uidsFile;
            StoredUids = storedUids;
            StatisticInfo = statisticInfo;
            TotalMessageCount = totalMessageCount;
            TotalMessageSize = totalMessageSize;
            CurrentMessageCount = currentMessageCount;
            CurrentMessageSize = currentMessageSize;
            RemainingMessageCount = remainingMessageCount;
            RemainingMessageSize = remainingMessageSize;
        }

        /// <summary>
        /// Gets the uids storage file.
        /// </summary>
        internal StorageFile UidsFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the stored uids.
        /// </summary>
        internal IList<string> StoredUids
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the statistic info.
        /// </summary>
        public StatisticInfo StatisticInfo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total message count.
        /// </summary>
        public uint TotalMessageCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the total message size.
        /// </summary>
        public uint TotalMessageSize
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the total message size format.
        /// </summary>
        public string TotalMessageSizeFormat
        {
            get
            {
                return IOUtil.GetMessageSize(TotalMessageSize);
            }
        }

        /// <summary>
        /// Gets the total message count.
        /// </summary>
        public uint CurrentMessageCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total message size.
        /// </summary>
        public uint CurrentMessageSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total message size format.
        /// </summary>
        public string CurrentMessageSizeFormat
        {
            get
            {
                return IOUtil.GetMessageSize(CurrentMessageSize);
            }
        }

        /// <summary>
        /// Gets the remaining message count.
        /// </summary>
        public uint RemainingMessageCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the remaining message size.
        /// </summary>
        public uint RemainingMessageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the remaining message size format.
        /// </summary>
        public string RemainingMessageSizeFormat
        {
            get
            {
                return IOUtil.GetMessageSize(RemainingMessageSize);
            }
        }
    }
}
