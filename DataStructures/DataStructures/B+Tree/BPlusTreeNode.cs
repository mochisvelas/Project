using System;
using System.Collections.Generic;

namespace DataStructures {

    class BPlusTreeNode<K, V> where K : IComparable<K> {

        // Properties
        private BPlusChilds<K, V> _childs;
        public BPlusTreeNode<K, V> parentNode { get; set; }
        public BPlusTreeNode<K, V> rightNode { get; set; }
        public BPlusTreeNode<K, V> leftNode { get; set; }
        public BPlusTreeNode<K, V> rightLeafNode { get; set; }
        public BPlusTreeNode<K, V> leftLeafNode { get; set; }
        public BPlusChilds<K, V> childs {
            get { return _childs; }
            set {
                _childs = value;
                if(_childs != null) {
                    foreach(KeyValuePair<K, BPlusTreeNode<K, V>> pair in _childs) {
                        pair.Value.parentNode = this;
                    }
                }
            }
        }
        public BPlusValues<K, V> values { get; set; }
        public BPlusTree<V, K> parentTree { get; set; }
        public bool isLeaf { get; set; }
        public bool isRoot { get; set; }

        /// <summary>Get the number of keys.</summary>
        public int KeysNumber {
            get {
                if (isLeaf) {
                    return values.Keys.Count;
                }
                return childs.Keys.Count;
            }
        }

        /// <summary>Get the maximun degree.</summary>
        public int MaximunDegree => parentTree.MaxDegree;

        /// <summary>Get the alpha value.</summary>
        public double Alpha => parentTree.Alpha;

        /// <summary>Get the minimun degree.</summary>
        public int MinimunDegree => parentTree.MinDegree;
        
        /// <summary>Contructor.</summary>
        /// <param name="childs">The childs of the node.</param>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="rightNode">The right node.</param>
        /// <param name="leftNode">The left node.</param>
        /// <param name="parentTree">The parent tree of the node.</param>
        /// <param name="isRoot">If the node is root.</param>
        public BPlusTreeNode(BPlusChilds<K, V> childs, BPlusTreeNode<K, V> parentNode, BPlusTreeNode<K, V> rightNode, BPlusTreeNode<K, V> leftNode, BPlusTree<V, K> parentTree, bool isRoot) {
            this.childs = childs;
            this.parentNode = parentNode;
            this.rightNode = rightNode;
            this.leftNode = leftNode;
            this.parentTree = parentTree;
            isLeaf = false;
            this.isRoot = isRoot;
        }

        /// <summary>Constructor.</summary>
        /// <param name="values">The values of the node.</param>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="rightLeafNode">The right leaf node.</param>
        /// <param name="leftLeafNode">The left leaf node.</param>
        /// <param name="parentTree">The parent tree of the node.</param>
        /// <param name="isRoot">If the node is root.</param>
        public BPlusTreeNode(BPlusValues<K, V> values, BPlusTreeNode<K, V> parentNode, BPlusTreeNode<K, V> rightLeafNode, BPlusTreeNode<K, V> leftLeafNode, BPlusTree<V, K> parentTree, bool isRoot) {
            this.values = values;
            this.parentNode = parentNode;
            this.rightLeafNode = rightLeafNode;
            this.leftLeafNode = leftLeafNode;
            this.parentTree = parentTree;
            isLeaf = true;
            this.isRoot = isRoot;
        }

    }

}

