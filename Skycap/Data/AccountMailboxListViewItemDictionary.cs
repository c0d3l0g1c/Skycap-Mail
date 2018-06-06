using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using Skycap.Net.Common;

namespace Skycap.Data
{
    /// <summary>
    /// Represents a collection of AccountSettingsData key and ObservableCollection<MailboxListViewItem> value pairs.
    /// </summary>
    public class AccountMailboxListViewItemDictionary : IDictionary<AccountSettingsData, ObservableCollection<MailboxListViewItem>>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Stores the AccountSettingsData and ObservableCollection<MailboxListViewItem> dictionary.
        /// </summary>
        private IDictionary<AccountSettingsData, ObservableCollection<MailboxListViewItem>> dictionary;

        /// <summary>
        /// Initializes a new instance of the Skycap.Data.AccountMailboxDictionary class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.
        /// </summary>
        public AccountMailboxListViewItemDictionary()
        {
            dictionary = new Dictionary<AccountSettingsData, ObservableCollection<MailboxListViewItem>>();
        }

        /// <summary>
        /// Gets an Skycap.Data.AccountMailboxDictionary containing the keys of the Skycap.Data.AccountMailboxDictionary.
        /// </summary>
        public ICollection<AccountSettingsData> Keys
        {
            get
            {
                return dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in the Skycap.Data.AccountMailboxDictionary.
        /// </summary>
        public ICollection<ObservableCollection<MailboxListViewItem>> Values
        {
            get
            {
                return dictionary.Values;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the Skycap.Data.AccountMailboxDictionary.
        /// </summary>
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Skycap.Data.AccountMailboxDictionary is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return dictionary.IsReadOnly;
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the Skycap.Data.AccountMailboxDictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(AccountSettingsData key, ObservableCollection<MailboxListViewItem> value)
        {
            dictionary.Add(key, value);

            OnPropertyChanged(this, new PropertyChangedEventArgs("Keys"));
            OnPropertyChanged(this, new PropertyChangedEventArgs("Values"));
            OnPropertyChanged(this, new PropertyChangedEventArgs("Count"));

            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, dictionary[key]));
        }

        /// <summary>
        /// Adds the specified key and value pair to the dictionary.
        /// </summary>
        /// <param name="item">The element to add.</param>
        public void Add(KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>> item)
        {
            dictionary.Add(item);

            OnPropertyChanged(this, new PropertyChangedEventArgs("Keys"));
            OnPropertyChanged(this, new PropertyChangedEventArgs("Values"));
            OnPropertyChanged(this, new PropertyChangedEventArgs("Count"));

            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, dictionary[item.Key]));
        }

        /// <summary>
        /// Determines whether the Skycap.Data.AccountMailboxDictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the Skycap.Data.AccountMailboxDictionary.</param>
        /// <returns>true if the Skycap.Data.AccountMailboxDictionary contains an element with the key; otherwise, false.</returns>
        public bool ContainsKey(AccountSettingsData key)
        {
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Removes the element with the specified key from the Skycap.Data.AccountMailboxDictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the original Skycap.Data.AccountMailboxDictionary.</returns>
        public bool Remove(AccountSettingsData key)
        {
            ObservableCollection<MailboxListViewItem> value = dictionary[key];

            if (dictionary.Remove(key))
            {
                OnPropertyChanged(this, new PropertyChangedEventArgs("Keys"));
                OnPropertyChanged(this, new PropertyChangedEventArgs("Values"));
                OnPropertyChanged(this, new PropertyChangedEventArgs("Count"));

                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements Skycap.Data.AccountMailboxDictionary contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(AccountSettingsData key, out ObservableCollection<MailboxListViewItem> value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a System.Collections.Generic.KeyNotFoundException, and a set operation creates a new element with the specified key.</returns>
        public ObservableCollection<MailboxListViewItem> this[AccountSettingsData key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                if (dictionary[key] == value)
                    return;

                ObservableCollection<MailboxListViewItem> old = dictionary[key];
                dictionary[key] = value;

                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old));
            }
        }

        /// <summary>
        /// Removes all keys and values from the Skycap.Data.AccountMailboxDictionary.
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();

            OnPropertyChanged(this, new PropertyChangedEventArgs("Keys"));
            OnPropertyChanged(this, new PropertyChangedEventArgs("Values"));
            OnPropertyChanged(this, new PropertyChangedEventArgs("Count"));

            OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determines whether the Skycap.Data.AccountMailboxDictionary contains the specified item.
        /// </summary>
        /// <param name="item">The item to locate in the Skycap.Data.AccountMailboxDictionary.</param>
        /// <returns>true if the Skycap.Data.AccountMailboxDictionary contains the specified item; otherwise, false.</returns>
        public bool Contains(KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>> item)
        {
            return dictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the Skycap.Data.AccountMailboxDictionary to an System.Array, starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from Skycap.Data.AccountMailboxDictionary. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the Skycap.Data.AccountMailboxDictionary.
        /// </summary>
        /// <param name="item">The object to remove from the Skycap.Data.AccountMailboxDictionary.</param>
        /// <returns>true if item was successfully removed from the Skycap.Data.AccountMailboxDictionary; otherwise, false. This method also returns false if item is not found in the original Skycap.Data.AccountMailboxDictionary.</returns>
        public bool Remove(KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>> item)
        {
            if (dictionary.Remove(item))
            {
                OnPropertyChanged(this, new PropertyChangedEventArgs("Keys"));
                OnPropertyChanged(this, new PropertyChangedEventArgs("Values"));
                OnPropertyChanged(this, new PropertyChangedEventArgs("Count"));

                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A System.Collections.Generic.IEnumerator<T> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<AccountSettingsData, ObservableCollection<MailboxListViewItem>>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns> An System.Collections.IEnumerator object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="sender">The object that raised the event (AccountMailboxDictionary).</param>
        /// <param name="e">The event data (NotifyCollectionChangedEventArgs).</param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="sender">The object that raised the event (AccountMailboxDictionary).</param>
        /// <param name="e">The event data (PropertyChangedEventArgs).</param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
