using System;
using System.Collections.Generic;

namespace DataStructures {

    class BPlusValues<K, V> : SortedList<K, V> where K : IComparable<K> {

        /// <summary>Add a renge of values.</summary>
        /// <param name="values">The values to add.</param>
        public void AddRange(IEnumerable<KeyValuePair<K, V>> values) {
            foreach (KeyValuePair<K, V> pair in values) {
                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>Copy to another node.</summary>
        /// <param name="values">The node to copy.</param>
        /// <param name="startIndex">The index to start.</param>
        /// <param name="endIndex">The index to end.</param>
        public void CopyTo(BPlusValues<K, V> values, int startIndex, int endIndex) {
            try {
                for (int i = startIndex; i < endIndex; i++) {
                    values.Add(Keys[i], Values[i]);
                }
            } catch (Exception) {
                throw new IndexOutOfRangeException("Index run out of range while copingBPlusValues.");
            }
        }

    }

}
