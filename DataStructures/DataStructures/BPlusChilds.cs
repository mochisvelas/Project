using System;
using System.Collections.Generic;

namespace DataStructures {

    class BPlusChilds<K, V> : SortedList<K, BPlusTreeNode<K, V>> where K : IComparable<K> {

        /// <summary>Add with parent.</summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="key">The key of the node.</param>
        /// <param name="node">The node to add.</param>
        public void Add(BPlusTreeNode<K, V> parent, K key, BPlusTreeNode<K, V> node) {
            Add(parent, null, null, key, node);
        }

        /// <summary>Add with parent and neighbours.</summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="leftNode">The left neighbour.</param>
        /// <param name="rightNode">The right neighbour.</param>
        /// <param name="key">The key of the node.</param>
        /// <param name="node">The node to add.</param>
        public void Add(BPlusTreeNode<K, V> parent, BPlusTreeNode<K, V> leftNode, BPlusTreeNode<K, V> rightNode, K key, BPlusTreeNode<K, V> node) {
            Add(key, node);
            node.parentNode = parent;
            node.parentTree = parent.parentTree;
            node.rightNode = rightNode;
            node.leftNode = leftNode;
        }
        
        /// <summary>Add a collection.</summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="collection">The collection to add.</param>
        public void AddRange(BPlusTreeNode<K, V> parent, IEnumerable<KeyValuePair<K, BPlusTreeNode<K, V>>> collection) {
            foreach (KeyValuePair<K, BPlusTreeNode<K, V>> pair in collection) {
                Add(pair.Key, pair.Value);
                pair.Value.parentNode = parent;
            }
        }
        
        /// <summary>Add a collection at the end.</summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="collection">The collection to add.</param>
        public void AddRangeToEnd(BPlusTreeNode<K, V> parent, IEnumerable<KeyValuePair<K, BPlusTreeNode<K, V>>> collection) {
            if (collection != null) {
                int index = Count;

                foreach (KeyValuePair<K, BPlusTreeNode<K, V>> pair in collection) {
                    Add(pair.Key, pair.Value);
                }

                for (int i = index; i < Count; i++) {
                    Values[i].parentNode = parent;
                    Values[i].parentTree = parent.parentTree;
                    if (i != 0) {
                        Values[i].leftNode = Values[i - 1];
                        Values[i - 1].rightNode = Values[i];
                    }
                }
                Values[Values.Count - 1].rightNode = null;
            }
        }

        /// <summary>Add a collection at the begin.</summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="collection">The collection to add.</param>
        public void AddRangeToBegin(BPlusTreeNode<K, V> parent, IEnumerable<KeyValuePair<K, BPlusTreeNode<K, V>>> collection) {
            if (collection != null) {
                int index = Count;

                foreach (KeyValuePair<K, BPlusTreeNode<K, V>> pair in collection) {
                    Add(pair.Key, pair.Value);
                }

                for (int i = 0; i < Count - index; i++) {
                    Values[i].parentNode = parent;
                    Values[i].parentTree = parent.parentTree;
                    if (i != Count - 1) {
                        Values[i].rightNode = Values[i + 1];
                        Values[i + 1].leftNode = Values[i];
                    }
                }
                Values[0].leftNode = null;
            }
        }

        /// <summary>Copy to another list.</summary>
        /// <param name="childs">The list to copy.</param>
        /// <param name="startIndex">The index to start.</param>
        /// <param name="endIndex">The index to end.</param>
        public void CopyTo(BPlusChilds<K, V> childs, int startIndex, int endIndex) {
            try {
                for (int i = startIndex; i < endIndex; i++) {
                    childs.Add(Keys[i], Values[i]);
                }
            } catch (Exception) {
                throw new IndexOutOfRangeException("Index run out of range while coping BPlusChilds.");
            }
        }

    }

}
