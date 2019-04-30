using System;

namespace DataStructures {

    class BSTNode<K, V> where K : IComparable<K> {

        // Properties
        public K key;
        public V value;
        public BSTNode<K, V> parent;
        public BSTNode<K, V> leftChild;
        public BSTNode<K, V> rightChild;

        /// <summary>Constructor</summary>
        /// <param name="key">The new key</param>
        /// <param name="value">The new value</param>
        public BSTNode(K key, V value) {
            this.key = key;
            this.value = value;
            parent = null;
            leftChild = null;
            rightChild = null;
        }

        /// <summary>Constructor</summary>
        /// <param name="key">The new key</param>
        /// <param name="value">The new value</param>
        /// <param name="parent">The parent node</param>
        public BSTNode(K key, V value, BSTNode<K, V> parent) {
            this.key = key;
            this.value = value;
            this.parent = parent;
            leftChild = null;
            rightChild = null;
        }

    }

}
