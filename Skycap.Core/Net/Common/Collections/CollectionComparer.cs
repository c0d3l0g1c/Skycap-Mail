using System.Collections.Generic;

namespace Skycap.Net.Common.Collections
{
    /// <summary>
    /// Represents a service used to compare two collections for equality.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collections.</typeparam>
    public static class CollectionComparer
    {
        /// <summary>
        /// Compares the content of two collections for equality.
        /// </summary>
        /// <param name="foo">The first collection.</param>
        /// <param name="bar">The second collection.</param>
        /// <returns>True if both collections have the same content, false otherwise.</returns>
        public static bool Compare<T>(ICollection<T> first, ICollection<T> second)
        {
            // Declare a dictionary to count the occurence of the items in the collection
            Dictionary<T, int> itemCounts = new Dictionary<T, int>();

            // Increase the count for each occurence of the item in the first collection
            foreach (T item in first)
            {
                if (itemCounts.ContainsKey(item))
                {
                    itemCounts[item]++;
                }
                else
                {
                    itemCounts[item] = 1;
                }
            }

            // Wrap the keys in a searchable list
            List<T> keys = new List<T>(itemCounts.Keys);

            // Decrease the count for each occurence of the item in the second collection
            foreach (T item in second)
            {
                // Try to find a key for the item
                // The keys of a dictionary are compared by reference, so we have to
                // find the original key that is equivalent to the "item"
                // You may want to override ".Equals" to define what it means for
                // two "T" objects to be equal
                T key = keys.Find(
                    delegate(T listKey)
                    {
                        return listKey.Equals(item);
                    });

                // Check if a key was found
                if (key != null)
                {
                    itemCounts[key]--;
                }
                else
                {
                    // There was no occurence of this item in the first collection, thus the collections are not equal
                    return false;
                }
            }

            // The count of each item should be 0 if the contents of the collections are equal
            foreach (int value in itemCounts.Values)
            {
                if (value != 0)
                {
                    return false;
                }
            }

            // The collections are equal
            return true;
        }
    }
}
