using System;

namespace DataStructures {

    class Element<K, V> where K : IComparable<K> {

        // Properties
        public K key { get; private set; }
        public V value { get; private set; }

        /// <summary>Constructor.</summary>
        /// <param name="key">The key of the element.</param>
        /// <param name="value">The value of the element.</param>
        public Element(K key, V value) {
            this.key = key;
            this.value = value;
        }

    }

}