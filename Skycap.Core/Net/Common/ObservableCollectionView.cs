using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Skycap.Net.Common
{
    /// <summary>
    /// Represents a dynamic data collection that can be filtered and sorted and provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [CollectionDataContract]
    public class ObservableCollectionView<T> : SortedObservableCollection<T>
    {
        /// <summary>
        /// The view.
        /// </summary>
        private ObservableCollectionView<T> _view;
        /// <summary>
        /// The filter.
        /// </summary>
        private Predicate<T> _filter;

        /// <summary>
        /// Initializes a new instance of the Skycap.Data.ObservableCollectionView<T> class.
        /// </summary>
        public ObservableCollectionView()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the Skycap.Data.ObservableCollectionView<T> class.
        /// </summary>
        /// <param name="comparer">The comparer to use for the sorting.</param>
        public ObservableCollectionView(IComparer<T> comparer)
            : base(comparer)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Skycap.Data.ObservableCollectionView<T> class.
        /// </summary>
        /// <param name="comparer">The comparer to use for the sorting.</param>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public ObservableCollectionView(IComparer<T> comparer, IEnumerable<T> collection)
            : base(comparer, collection)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Skycap.Data.ObservableCollectionView<T> class.
        /// </summary>
        /// <param name="comparer">The comparer to use for the sorting.</param>
        /// <param name="collection">The collection from which the elements are copied.</param>
        /// <param name="filter">The filter.</param>
        public ObservableCollectionView(IComparer<T> comparer, IEnumerable<T> collection, Predicate<T> filter)
            : base(comparer, collection == null ? new T[] { } : collection)
        {
            if (filter != null)
            {
                _filter = filter;

                if (collection == null)
                    _view = new ObservableCollectionView<T>(comparer);
                else
                    _view = new ObservableCollectionView<T>(comparer, collection);
            }
        }

        /// <summary>
        /// Gets the filtered view.
        /// </summary>
        [IgnoreDataMember]
        public ObservableCollectionView<T> View
        {
            get
            { 
                return (_filter == null ? this : _view);
            }
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        [IgnoreDataMember]
        public Predicate<T> Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                if (value == null)
                {
                    _filter = null;
                    _view = new ObservableCollectionView<T>(base.Comparer);
                }
                else
                {
                    _filter = value;
                    Fill();
                }
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            // Clear the view
            _view.Clear();

            // Do base ClearItems
            base.ClearItems();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            // Add to view
            FilterCheck(item, () =>
            {
                _view.Add(item);
            });

            // Do base InsertItem
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Removes the element at the specified index of the System.Collections.ObjectModel.Collection<T>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            // Get the item
            T item = this[index];

            // Remove from view
            FilterCheck(item, () =>
            {
                _view.Remove(item);
            });

            // Do base RemoveItem
            base.RemoveItem(index);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        protected override void SetItem(int index, T item)
        {
            // Get the item
            T oldItem = this[index];

            // Update view value
            FilterCheck(item, () =>
            {
                _view[GetViewItemIndex(oldItem)] = item;
            });

            // Do base SetItem
            base.SetItem(index, item);
        }

        /// <summary>
        /// Fills this collection with items from the source collection that meet the filter requirements.
        /// </summary>
        private void Fill()
        {
            // Clear the collection
            _view = new ObservableCollectionView<T>(base.Comparer);
            // For each item in the source
            Parallel.ForEach(this, item =>
            {
                // Add to view
                FilterCheck(item, () =>
                {
                    _view.Add(item);
                });
            });
        }

        /// <summary>
        /// Gets or sets the index of the specified element.
        /// </summary>
        /// <param name="item">The element to get or set.</param>
        /// <returns>The index of the specified element.</returns>
        private int GetViewItemIndex(T item)
        {
            // Default to -1; not found
            int foundIndex = -1;

            // Loop through each item
            for (int index = 0; index < View.Count; index++)
            {
                // If found
                if (View[index].Equals(item))
                {
                    // Get the index
                    foundIndex = index;
                    break;
                }
            }

            // Return found index
            return foundIndex;
        }

        /// <summary>
        /// Does the filter check.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="action">The action to take.</param>
        private void FilterCheck(T item, Action action)
        {
            if (_filter != null
             && _filter(item))
                action();
        }
    }
}