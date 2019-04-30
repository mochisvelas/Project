using System;
using System.Collections.Generic;

namespace DataStructures {

    public class BST<V, K> where K : IComparable<K> {

        // Properties
        private BSTNode<K, V> root;
        public int Count { get; private set; }

        /// <summary>Constructor.</summary>
        public BST() {
            root = null;
            Count = 0;
        }

        /// <summary>Insert a new element in the tree.</summary>
        /// <param name="key">The key for the new element</param>
        /// <param name="value">The value for the new element</param>
        public void Insert(K key, V value) {
            if (root == null) {
                root = new BSTNode<K, V>(key, value);
            } else {
                root = Insert(key, value, root);
            }
            Count++;
        }

        /// <summary>Remove the node.</summary>
        /// <param name="key">The key of the node to remove.</param>
        public void Remove(K key) {
            root = Delete(key, root);
            Count--;
        }

        /// <summary>Find the node with the same key.</summary>
        /// <param name="key">The key of the node.</param>
        /// <returns>The value of the node.</returns>
        public V Find(K key) {
            BSTNode<K, V> BSTNode = Find(root, key);
            if (BSTNode == null) {
                return default(V);
            } else {
                return BSTNode.value;
            }
        }

        /// <summary>Clear the tree</summary>
        public void Clear() {
            root = null;
        }

        /// <summary>Pass the tree to a list</summary>
        /// <returns>The tree in a sorted list</returns>
        public List<V> ToList() {
            List<V> list = new List<V>();
            InOrder(root, ref list);
            return list;
        }

        /// <summary>Update a node in the tree.</summary>
        /// <param name="key">The key of the node.</param>
        /// <param name="value">The new value of the node.</param>
        public void Update(K key, V value) {
            if (root != null) {
                root = Update(root, key, value);
            }
        }

        /// <summary>Check if the tree is empty.</summary>
        /// <returns>True if is empty.</returns>
        public bool IsEmpty() {
            if (root == null)
                return true;
            else
                return false;
        }

        /// <summary>Find the place to add the new item.</summary>
        /// <param name="key">The value to compare in the tree.</param>
        /// <param name="value">Data of the new node in the tree.</param>
        /// <param name="currentBSTNode">the current node in the tree.</param>
        /// <returns>The new structure of the tree.</returns>
        private BSTNode<K, V> Insert(K key, V value, BSTNode<K, V> currentBSTNode) {
            if (currentBSTNode == null) {
                currentBSTNode = new BSTNode<K, V>(key, value);
                return currentBSTNode;
            } else if (key.CompareTo(currentBSTNode.key) <= 0) {
                currentBSTNode.leftChild = Insert(key, value, currentBSTNode.leftChild);
            } else {
                currentBSTNode.rightChild = Insert(key, value, currentBSTNode.rightChild);
            }
            return currentBSTNode;
        }
        
        /// <summary>Find and delete a item in the tree.</summary>
        /// <param name="key">The value to compare in the tree.</param>
        /// <param name="currentBSTNode">the current node in the tree.</param>
        /// <returns>The new structure of the tree.</returns>
        private BSTNode<K, V> Delete(K key, BSTNode<K, V> currentBSTNode) {
            if (currentBSTNode == null) {
                return null;
            } else {
                if (key.CompareTo(currentBSTNode.key) < 0) {
                    currentBSTNode.leftChild = Delete(key, currentBSTNode.leftChild);
                } else if (key.CompareTo(currentBSTNode.key) > 0) {
                    currentBSTNode.rightChild = Delete(key, currentBSTNode.rightChild);
                } else {
                    if (currentBSTNode.leftChild == null && currentBSTNode.rightChild == null) {
                        currentBSTNode = null;
                        return currentBSTNode;
                    } else if (currentBSTNode.rightChild == null) {
                        currentBSTNode = currentBSTNode.leftChild;
                    } else if (currentBSTNode.leftChild == null) {
                        currentBSTNode = currentBSTNode.rightChild;
                    } else {
                        BSTNode<K, V> minValue = FindMinimun(currentBSTNode.rightChild);
                        currentBSTNode.key = minValue.key;
                        currentBSTNode.value = minValue.value;
                        currentBSTNode.rightChild = Delete(minValue.key, currentBSTNode.rightChild);
                    }
                }
            }
            return currentBSTNode;
        }

        /// <summary>Find the minimun value in the subtree.</summary>
        /// <param name="currentBSTNode">The current node.</param>
        /// <returns>The node with the minimun value.</returns>
        private BSTNode<K, V> FindMinimun(BSTNode<K, V> currentBSTNode) {
            if (currentBSTNode == null) {
                return null;
            } else if (currentBSTNode.leftChild == null) {
                return currentBSTNode;
            }
            return FindMinimun(currentBSTNode.leftChild);
        }

        /// <summary>Find and return the value of the node.</summary>
        /// <param name="currentBSTNode">The current node.</param>
        /// <param name="key">The key of the node.</param>
        /// <returns>The found node.</returns>
        private BSTNode<K, V> Find(BSTNode<K, V> currentBSTNode, K key) {
            if (currentBSTNode == null || currentBSTNode.key.CompareTo(key) == 0) {
                return currentBSTNode;
            } if (key.CompareTo(currentBSTNode.key) > 0) {
                return Find(currentBSTNode.rightChild, key);
            }
            return Find(currentBSTNode.leftChild, key);
        }

        /// <summary>Iterate the tree and add to a list</summary>
        /// <param name="currentBSTNode">The current node.</param>
        /// <param name="list">The list to save the element.</param>
        private void InOrder(BSTNode<K, V> currentBSTNode, ref List<V> list) {
            if (currentBSTNode != null) {
                InOrder(currentBSTNode.leftChild, ref list);
                list.Add(currentBSTNode.value);
                InOrder(currentBSTNode.rightChild, ref list);
            }
        }

        /// <summary>Find the node to update.</summary>
        /// <param name="currentBSTNode">The current node.</param>
        /// <param name="key">The key of the node.</param>
        /// <param name="value">The new value of the node.</param>
        /// <returns>The node found.</returns>
        private BSTNode<K, V> Update(BSTNode<K, V> currentBSTNode, K key, V value) {
            if (currentBSTNode.key.CompareTo(key) == 0) {
                currentBSTNode.value = value;
                return currentBSTNode;
            } else if (key.CompareTo(currentBSTNode.key) <= 0) {
                currentBSTNode.leftChild = Update(currentBSTNode.leftChild, key, value);
            } else {
                currentBSTNode.rightChild = Update(currentBSTNode.rightChild, key, value);
            }
            return currentBSTNode;
        }

    }

}
