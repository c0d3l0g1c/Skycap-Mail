using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents a mail header collection that can be filtered and sorted and provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    [CollectionDataContract]
    public class MailHeaderObservableCollectionView : ObservableCollectionView<MailHeader>
    {
        /// <summary>
        /// Initialises a new instance of the Skycap.Data.MailHeaderObservableCollectionView class.
        /// </summary>
        public MailHeaderObservableCollectionView()
            : base(new MailHeaderComparer())
        {
            // Initialise local properties
            base.Filter = null;
        }

        /// <summary>
        /// Initialises a new instance of the Skycap.Data.MailHeaderObservableCollectionView class.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public MailHeaderObservableCollectionView(IEnumerable<MailHeader> collection)
            : base(new MailHeaderComparer(), collection)
        {
            // Initialise local properties
            base.Filter = null;
        }

        /// <summary>
        /// Gets the unread email count.
        /// </summary>
        [IgnoreDataMember]
        public int UnreadEmailCount
        {
            get
            {
                lock (base.View)
                {
                    return base.View.Count(o => !o.IsSeen);
                }
            }
        }
    }
}
