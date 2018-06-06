using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents a dynamic sorted data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [CollectionDataContract]
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the Skycap.Data.SortedObservableCollection<T> class.
        /// </summary>
        public SortedObservableCollection()
            : base()
        { 
        
        }

        /// <summary>
        /// Initializes a new instance of the Skycap.Data.SortedObservableCollection<T> class.
        /// </summary>
        /// <param name="comparer">The comparer to use for the sorting.</param>
        public SortedObservableCollection(IComparer<T> comparer)
            : base()
        {
            // Initialise local variables
            Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the Skycap.Data.SortedObservableCollection<T> class.
        /// </summary>
        /// <param name="comparer">The comparer to use for the sorting.</param>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public SortedObservableCollection(IComparer<T> comparer, IEnumerable<T> collection)
            : base(collection)
        {
            // Initialise local variables
            Comparer = comparer;
        }

        /// <summary>
        /// Gets the comparer to use for the sorting.
        /// </summary>
        [IgnoreDataMember]
        protected IComparer<T> Comparer
        {
            get;
            private set;
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            lock (this)
            {
                index = Array.BinarySearch<T>(this.ToArray(), item, Comparer);
                base.InsertItem(~index, item);
            }
        }
    }
}