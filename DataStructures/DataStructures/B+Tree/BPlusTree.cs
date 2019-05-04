using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures {

    public class BPlusTree<V, K> where K : IComparable<K> {

        // Properties
        private int maxDegree;
        private double alpha;
        private BPlusTreeNode<K, V> root;
        public int Count { get; private set; }
        public int NodesCount { get; private set; }

        /// <summary>Get the max degre of the tree.</summary>
        public int MaxDegree {
            get { return maxDegree; }
            private set { maxDegree = value < 4 ? 4 : value; }
        }

        /// <summary>Get the alpha value</summary>
        public double Alpha {
            get { return alpha; }
            private set { alpha = (value <= 0.5 && value > 0) ? value : 0.5; }
        }

        /// <summary>Get the minimun degree of the tree.</summary>
        public int MinDegree {
            get { return ((int)(MaxDegree * Alpha) >= 2) ? (int)(MaxDegree * Alpha) : 2; }
        }

        /// <summary>Get the number of keys</summary>
        public int KeysCount {
            get { return Count; }
        }

        /// <summary>Constructor.</summary>
        /// <param name="maxDegree">The degree of the tree.</param>
        /// <param name="alpha">The alpha value.</param>
        public BPlusTree(int degree, double alpha) {
            maxDegree = degree;
            this.alpha = alpha;
        }

        /// <summary>Insert a new element in the tree.</summary>
        /// <param name="key">The key of the element.</param>
        /// <param name="value">The value of the element.</param>
        public void Insert(K key, V value) {
            Add(new KeyValuePair<K, V>(key, value));
        }

        /// <summary>Remove an element in the tree.</summary>
        /// <param name="key">The key of the element.</param>
        public void Remove(K key) {
            BPlusTreeNode<K, V> current = FindLeaf(key);
            if (current != null) {
                if (current.values.ContainsKey(key)) {
                    Count--;
                    if (current.values.Keys.Max().CompareTo(key) == 0 && current.parentNode != null) {
                        current.values.Remove(key);
                        current.parentNode.childs.Remove(key);
                        current.parentNode.childs.Add(current.values.Keys.Max(), current);
                        BPlusTreeNode<K, V> parent = current.parentNode;
                        while (parent.childs.Keys.Max().CompareTo(current.values.Keys.Max()) == 0 && parent.parentNode != null) {
                            parent.parentNode.childs.Remove(key);
                            parent.parentNode.childs.Add(current.values.Keys.Max(), parent);
                            parent = parent.parentNode;
                        }
                    } else {
                        current.values.Remove(key);
                    }
                    if (current.KeysNumber < MinDegree && !current.isRoot) {
                        if (current.isLeaf) {
                            MergeOrPullLeaf(current);
                        } else {
                            MergeOrPullNode(current);
                        }
                    }
                    if (current.isRoot && current.KeysNumber == 0) {
                        NodesCount--;
                        root = null;
                    }
                }
            }
        }

        /// <summary>Find an element in the tree.</summary>
        /// <param name="key">The key of the element.</param>
        /// <returns>The value of the element.</returns>
        public V Find(K key) {
            BPlusTreeNode<K, V> leaf = FindLeaf(key);
            V value = default(V);
            if (leaf != null && FindValue(leaf, key, out value)) {
                return value;
            }
            return default(V);
        }

        /// <summary>Clear the tree.</summary>
        public void Clear() {
            Count = 0;
            NodesCount = 0;
            root = null;
        }

        /// <summary>Add the tree in a list.</summary>
        /// <returns>The list with the tree elements.</returns>
        public List<V> ToList() {
            List<V> list = new List<V>();
            ConvertList(root, ref list);
            return list;
        }

        /// <summary>Update the value of a element in the tree.</summary>
        /// <param name="key">The key of the element.</param>
        /// <param name="value">The new value of the element.</param>
        public void Update(K key, V value) {
            if(root != null) {
                UpdateItem(root, key, value);
            }
        }

        /// <summary>Check if the tree is empty.</summary>
        /// <returns>True if is empty</returns>
        public bool IsEmpty() {
            if (root == null)
                return true;
            else
                return false;
        }

        /// <summary>Add a new element in the tree.</summary>
        /// <param name="pair">The element to add.</param>
        private void Add(KeyValuePair<K, V> pair) {
            BPlusTreeNode<K, V> current = FindLeaf(pair.Key);
            if (current != null) {
                if (current.values.Last().Key.CompareTo(pair.Key) < 0 && current.parentNode != null) {
                    K oldKey = current.values.Keys.Last();
                    current.values.Add(pair.Key, pair.Value);
                    current.parentNode.childs.Remove(oldKey);
                    current.parentNode.childs.Add(pair.Key, current);
                    BPlusTreeNode<K, V> parent = current.parentNode;
                    while (parent.parentNode != null) {
                        parent.parentNode.childs.Remove(oldKey);
                        parent.parentNode.childs.Add(pair.Key, parent);
                        parent = parent.parentNode;
                    }
                } else {
                    current.values.Add(pair.Key, pair.Value);
                }

                if (current.KeysNumber > MaxDegree) {
                    Split(current);
                }
            } else {
                BPlusValues<K, V> list = new BPlusValues<K, V>();
                list.Add(pair.Key, pair.Value);
                root = new BPlusTreeNode<K, V>(list, null, null, null, this, true);
                NodesCount++;
            }
            Count++;
        }

        /// <summary>Find leaf that might contain a key.</summary>
        /// <param name="key">The key to find.</param>
        /// <returns>The node found or null.</returns>
        private BPlusTreeNode<K, V> FindLeaf(K key) {
            if (root != null) {
                BPlusTreeNode<K, V> current = root;
                while (!current.isLeaf) {
                    current = current.childs.Values[FindIndex(current, key)];
                }
                return current;
            }
            return null;
        }

        /// <summary>Find the index of the child to go.</summary>
        /// <param name="node">The node to find the index.</param>
        /// <param name="key">The key to find.</param>
        /// <returns>The index to go.</returns>
        private int FindIndex(BPlusTreeNode<K, V> node, K key) {
            for (int i = 0; i < node.childs.Count; i++) {
                if (node.childs.Keys[i].CompareTo(key) >= 0) {
                    return i;
                }
            }
            return node.childs.Count - 1;
        }

        /// <summary>Split the node.</summary>
        /// <param name="node">The node to split.</param>
        private void Split(BPlusTreeNode<K, V> node) {
            NodesCount++;
            if (node.isLeaf) {
                SplitLeaf(node);
            } else {
                SplitNode(node);
            }
        }

        /// <summary>Split the leaf node.</summary>
        /// <param name="leafToSplit">The leaf node to split.</param>
        private void SplitLeaf(BPlusTreeNode<K, V> leafToSplit) {
            int pivotIndex = leafToSplit.values.Count / 2;
            BPlusValues<K, V> leftValues = new BPlusValues<K, V>();
            BPlusValues<K, V> rightValues = new BPlusValues<K, V>();
            leafToSplit.values.CopyTo(leftValues, 0, pivotIndex + 1);
            leafToSplit.values.CopyTo(rightValues, pivotIndex + 1, leafToSplit.values.Count);
            BPlusTreeNode<K, V> leftNode = new BPlusTreeNode<K, V>(leftValues, leafToSplit.parentNode, null, null, leafToSplit.parentTree, false);
            BPlusTreeNode<K, V> rightNode = new BPlusTreeNode<K, V>(rightValues, leafToSplit.parentNode, null, null, leafToSplit.parentTree, false);
            leftNode.rightLeafNode = rightNode;
            leftNode.leftLeafNode = leafToSplit.leftLeafNode;
            rightNode.leftLeafNode = leftNode;
            rightNode.rightLeafNode = leafToSplit.rightLeafNode;
            if (leafToSplit.leftLeafNode != null) {
                leafToSplit.leftLeafNode.rightLeafNode = leftNode;
            }
            if (leafToSplit.rightLeafNode != null) {
                leafToSplit.rightLeafNode.leftLeafNode = rightNode;
            }
            if (!leafToSplit.isRoot) {
                leafToSplit.parentNode.childs.Remove(leafToSplit.values.Keys.Max());
                leafToSplit.parentNode.childs.Add(rightNode.values.Keys.Last(), rightNode);
                leafToSplit.parentNode.childs.Add(leftNode.values.Keys.Last(), leftNode);
            } else {
                BPlusTreeNode<K, V> newRoot = new BPlusTreeNode<K, V>(new BPlusChilds<K, V>(), null, null, null, leafToSplit.parentTree, true);
                leftNode.parentNode = newRoot;
                rightNode.parentNode = newRoot;
                newRoot.childs.Add(leftNode.values.Keys.Last(), leftNode);
                newRoot.childs.Add(rightNode.values.Keys.Last(), rightNode);
                root = newRoot;
                NodesCount++;
            }
            if (leftNode.parentNode.KeysNumber > MaxDegree) {
                Split(leftNode.parentNode);
            }
        }

        /// <summary>Split a node.</summary>
        /// <param name="nodeToSplit">The node to split.</param>
        private void SplitNode(BPlusTreeNode<K, V> nodeToSplit) {
            int pivotIndex = nodeToSplit.childs.Count / 2;
            BPlusChilds<K, V> leftLinks = new BPlusChilds<K, V>();
            BPlusChilds<K, V> rightLinks = new BPlusChilds<K, V>();
            nodeToSplit.childs.CopyTo(leftLinks, 0, pivotIndex + 1);
            nodeToSplit.childs.CopyTo(rightLinks, pivotIndex + 1, nodeToSplit.childs.Count);
            BPlusTreeNode<K, V> leftNode = new BPlusTreeNode<K, V>(leftLinks, nodeToSplit.parentNode, null, null, nodeToSplit.parentTree, false);
            BPlusTreeNode<K, V> rightNode = new BPlusTreeNode<K, V>(rightLinks, nodeToSplit.parentNode, null, null, nodeToSplit.parentTree, false);
            leftNode.rightNode = rightNode;
            leftNode.leftNode = nodeToSplit.leftNode;
            rightNode.leftNode = leftNode;
            rightNode.rightNode = nodeToSplit.rightNode;
            if (nodeToSplit.leftNode != null) {
                nodeToSplit.leftNode.rightNode = leftNode;
            }
            if (nodeToSplit.rightNode != null) {
                nodeToSplit.rightNode.leftNode = rightNode;
            }
            if (!nodeToSplit.isRoot) {
                nodeToSplit.parentNode.childs.Remove(nodeToSplit.childs.Keys.Max());
                nodeToSplit.parentNode.childs.Add(rightNode.childs.Keys.Last(), rightNode);
                nodeToSplit.parentNode.childs.Add(leftNode.childs.Keys.Last(), leftNode);
            } else {
                BPlusTreeNode<K, V> newRoot = new BPlusTreeNode<K, V>(new BPlusChilds<K, V>(), null, null, null, nodeToSplit.parentTree, true);
                leftNode.parentNode = newRoot;
                rightNode.parentNode = newRoot;
                newRoot.childs.Add(leftNode.childs.Keys.Last(), leftNode);
                newRoot.childs.Add(rightNode.childs.Keys.Last(), rightNode);
                root = newRoot;
                NodesCount++;
            }
            if (leftNode.parentNode.KeysNumber > MaxDegree) {
                Split(leftNode.parentNode);
            }
        }

        /// <summary>Merge or pull a leaf.</summary>
        /// <param name="leaf">The node to merge or pull.</param>
        private void MergeOrPullLeaf(BPlusTreeNode<K, V> leaf) {
            if (leaf.rightLeafNode != null && leaf.leftLeafNode != null) {
                BPlusTreeNode<K, V> minLeaf = (leaf.leftLeafNode.KeysNumber < leaf.rightLeafNode.KeysNumber) ? leaf.leftLeafNode : leaf.rightLeafNode;
                BPlusTreeNode<K, V> maxLeaf = (leaf.leftLeafNode.KeysNumber >= leaf.rightLeafNode.KeysNumber) ? leaf.leftLeafNode : leaf.rightLeafNode;
                if (leaf.KeysNumber + minLeaf.KeysNumber <= MaxDegree) {
                    if (leaf.rightLeafNode == minLeaf) {
                        MergeLeaves(leaf, minLeaf);
                    } else {
                        MergeLeaves(minLeaf, leaf);
                    }
                } else {
                    PullLeaves(leaf, maxLeaf);
                }
            } else if (leaf.leftLeafNode != null) {
                if (leaf.KeysNumber + leaf.leftLeafNode.KeysNumber <= MaxDegree) {
                    MergeLeaves(leaf.leftLeafNode, leaf);
                } else {
                    PullLeaves(leaf, leaf.leftLeafNode);
                }
            } else if (leaf.rightLeafNode != null) {
                if (leaf.KeysNumber + leaf.rightLeafNode.KeysNumber <= MaxDegree) {
                    MergeLeaves(leaf, leaf.rightLeafNode);
                } else {
                    PullLeaves(leaf, leaf.rightLeafNode);
                }
            } else {
                if (!leaf.isRoot) {
                    leaf.isRoot = true;
                    leaf.parentNode = null;
                    root = leaf;
                    NodesCount--;
                }
            }
        }

        /// <summary>Merge or pull a node.</summary>
        /// <param name="node">The node to merge or pull.</param>
        private void MergeOrPullNode(BPlusTreeNode<K, V> node) {
            if (node.rightNode != null && node.leftNode != null) {
                BPlusTreeNode<K, V> minNode = (node.leftNode.KeysNumber < node.rightNode.KeysNumber) ? node.leftNode : node.rightNode;
                BPlusTreeNode<K, V> maxNode = (node.leftNode.KeysNumber >= node.rightNode.KeysNumber) ? node.leftNode : node.rightNode;
                if (node.KeysNumber + minNode.KeysNumber <= MaxDegree) {
                    if (node.rightNode == minNode) {
                        MergeNodes(node, minNode);
                    } else {
                        MergeNodes(minNode, node);
                    }
                } else {
                    PullNodes(node, maxNode);
                }
            } else if (node.leftNode != null) {
                if (node.KeysNumber + node.leftNode.KeysNumber <= MaxDegree) {
                    MergeNodes(node.leftNode, node);
                } else {
                    PullNodes(node, node.leftNode);
                }
            } else if (node.rightNode != null) {
                if (node.KeysNumber + node.rightNode.KeysNumber <= MaxDegree) {
                    MergeNodes(node, node.rightNode);
                } else {
                    PullNodes(node, node.rightNode);
                }
            } else {
                if (!node.isRoot) {
                    node.isRoot = true;
                    node.parentNode = null;
                    root = node;
                    NodesCount--;
                }
            }
        }

        /// <summary>Merge the leaves.</summary>
        /// <param name="leftLeafToMerge">The left leaves to merge.</param>
        /// <param name="rightLeafToMerge">The right leaves to merge.</param>
        private void MergeLeaves(BPlusTreeNode<K, V> leftLeafToMerge, BPlusTreeNode<K, V> rightLeafToMerge) {
            NodesCount--;
            BPlusTreeNode<K, V> newLeaf = new BPlusTreeNode<K, V>(new BPlusValues<K, V>(), null, null, null, leftLeafToMerge.parentTree, false);
            BPlusValues<K, V> mergedValues = new BPlusValues<K, V>();
            mergedValues.AddRange(leftLeafToMerge.values);
            mergedValues.AddRange(rightLeafToMerge.values);
            newLeaf.values = mergedValues;
            newLeaf.parentNode = rightLeafToMerge.parentNode;
            newLeaf.leftLeafNode = leftLeafToMerge.leftLeafNode;
            newLeaf.rightLeafNode = rightLeafToMerge.rightLeafNode;
            if (newLeaf.leftLeafNode != null) {
                newLeaf.leftLeafNode.rightLeafNode = newLeaf;
            }
            if (newLeaf.rightLeafNode != null) {
                newLeaf.rightLeafNode.leftLeafNode = newLeaf;
            }
            newLeaf.parentNode.childs.Remove(rightLeafToMerge.values.Keys.Max());
            newLeaf.parentNode.childs.Add(newLeaf.values.Keys.Max(), newLeaf);
            leftLeafToMerge.parentNode.childs.Remove(leftLeafToMerge.values.Keys.Max());
            if (leftLeafToMerge.parentNode != rightLeafToMerge.parentNode) {
                leftLeafToMerge.parentNode.parentNode.childs.Remove(leftLeafToMerge.values.Keys.Max());
                leftLeafToMerge.parentNode.parentNode.childs.Add(leftLeafToMerge.parentNode.childs.Keys.Max(), leftLeafToMerge.parentNode);
                if (leftLeafToMerge.parentNode.KeysNumber < MinDegree && !leftLeafToMerge.parentNode.isRoot) {
                    MergeOrPullNode(leftLeafToMerge.parentNode);
                }
            }
            if (newLeaf.parentNode.KeysNumber < MinDegree && !newLeaf.parentNode.isRoot) {
                MergeOrPullNode(newLeaf.parentNode);
            } else if (newLeaf.parentNode.isRoot && newLeaf.parentNode.KeysNumber == 1) {
                newLeaf.isRoot = true;
                newLeaf.parentNode = null;
                root = newLeaf;
                NodesCount--;
            }
        }

        /// <summary>Pull the leaves.</summary>
        /// <param name="leafToPull">The left leaf to pull.</param>
        /// <param name="leafFromPull">The right leaf to pull.</param>
        private void PullLeaves(BPlusTreeNode<K, V> leafToPull, BPlusTreeNode<K, V> leafFromPull) {
            int numberOfValuesToPull = leafToPull.KeysNumber + (leafFromPull.KeysNumber - leafToPull.KeysNumber) / 2;
            if (leafToPull.rightLeafNode == leafFromPull) {
                BPlusValues<K, V> pulledValues = new BPlusValues<K, V>();
                for (int i = 0; i < numberOfValuesToPull; i++) {
                    pulledValues.Add(leafFromPull.values.Keys[0], leafFromPull.values.Values[0]);
                    leafFromPull.values.RemoveAt(0);
                }
                K oldKey = leafToPull.values.Keys.Max();
                leafToPull.values.AddRange(pulledValues);
                leafToPull.parentNode.childs.Remove(oldKey);
                leafToPull.parentNode.childs.Add(leafToPull.values.Keys.Max(), leafToPull);
            } else {
                BPlusValues<K, V> pulledValues = new BPlusValues<K, V>();
                K oldKey = leafFromPull.values.Keys.Max();
                for (int i = 0; i < numberOfValuesToPull; i++) {
                    pulledValues.Add(leafFromPull.values.Keys[leafFromPull.KeysNumber - 1], leafFromPull.values.Values[leafFromPull.KeysNumber - 1]); leafFromPull.values.RemoveAt(leafFromPull.KeysNumber - 1);
                }
                leafToPull.values.AddRange(pulledValues);
                leafFromPull.parentNode.childs.Remove(oldKey);
                leafFromPull.parentNode.childs.Add(leafFromPull.values.Keys.Max(), leafFromPull);
            }
        }

        /// <summary>Merge the nodes.</summary>
        /// <param name="leftNodeToMerge">The left node to merge.</param>
        /// <param name="rightNodeToMerge">The right node to merge.</param>
        private void MergeNodes(BPlusTreeNode<K, V> leftNodeToMerge, BPlusTreeNode<K, V> rightNodeToMerge) {
            NodesCount--;
            BPlusTreeNode<K, V> newNode = new BPlusTreeNode<K, V>(new BPlusChilds<K, V>(), null, null, null, leftNodeToMerge.parentTree, false);
            BPlusChilds<K, V> mergedLinks = new BPlusChilds<K, V>();
            mergedLinks.AddRange(newNode, leftNodeToMerge.childs);
            mergedLinks.AddRange(newNode, rightNodeToMerge.childs);
            newNode.childs = mergedLinks;
            newNode.parentNode = rightNodeToMerge.parentNode;
            newNode.leftNode = leftNodeToMerge.leftNode;
            newNode.rightNode = rightNodeToMerge.rightNode;
            if (newNode.rightNode != null) {
                newNode.rightNode.leftNode = newNode;
            }
            if (newNode.leftNode != null) {
                newNode.leftNode.rightNode = newNode;
            }
            newNode.parentNode.childs.Remove(rightNodeToMerge.childs.Keys.Max());
            newNode.parentNode.childs.Add(newNode.childs.Keys.Max(), newNode);
            leftNodeToMerge.parentNode.childs.Remove(leftNodeToMerge.childs.Keys.Max());
            if (leftNodeToMerge.parentNode != rightNodeToMerge.parentNode) {
                leftNodeToMerge.parentNode.parentNode.childs.Remove(leftNodeToMerge.childs.Keys.Max());
                leftNodeToMerge.parentNode.parentNode.childs.Add(leftNodeToMerge.parentNode.childs.Keys.Max(), leftNodeToMerge.parentNode);
                if (leftNodeToMerge.parentNode.KeysNumber < MinDegree && !leftNodeToMerge.parentNode.isRoot) {
                    MergeOrPullNode(leftNodeToMerge.parentNode);
                }
            }
            if (newNode.parentNode.KeysNumber < MinDegree && !newNode.parentNode.isRoot) {
                MergeOrPullNode(newNode.parentNode);
            } else if (newNode.parentNode.isRoot && newNode.parentNode.KeysNumber == 1) {
                newNode.isRoot = true;
                newNode.parentNode = null;
                root = newNode;
                NodesCount--;
            }
        }

        /// <summary>Pull the nodes.</summary>
        /// <param name="nodeToPull">The node to pull.</param>
        /// <param name="nodeFromPull">The node from pull.</param>
        private void PullNodes(BPlusTreeNode<K, V> nodeToPull, BPlusTreeNode<K, V> nodeFromPull) {
            int numberOfLinksToPull = nodeToPull.KeysNumber + (nodeFromPull.KeysNumber - nodeToPull.KeysNumber) / 2;
            if (nodeToPull.rightNode == nodeFromPull) {
                BPlusChilds<K, V> pulledLinks = new BPlusChilds<K, V>();
                for (int i = 0; i < numberOfLinksToPull; i++) {
                    pulledLinks.Add(nodeFromPull.childs.Keys[0], nodeFromPull.childs.Values[0]);
                    nodeFromPull.childs.RemoveAt(0);
                }
                K oldKey = nodeToPull.childs.Keys.Max();
                nodeToPull.childs.AddRange(nodeToPull, pulledLinks);
                nodeToPull.parentNode.childs.Remove(oldKey);
                nodeToPull.parentNode.childs.Add(nodeToPull.childs.Keys.Max(), nodeToPull);
            } else {
                BPlusChilds<K, V> pulledLinks = new BPlusChilds<K, V>();
                K oldKey = nodeFromPull.childs.Keys.Max();
                for (int i = 0; i < numberOfLinksToPull; i++) {
                    pulledLinks.Add(nodeFromPull.childs.Keys[nodeFromPull.KeysNumber - 1], nodeFromPull.childs.Values[nodeFromPull.KeysNumber - 1]);
                    nodeFromPull.childs.RemoveAt(nodeFromPull.KeysNumber - 1);
                }
                nodeToPull.childs.AddRange(nodeToPull, pulledLinks);
                nodeFromPull.parentNode.childs.Remove(oldKey);
                nodeFromPull.parentNode.childs.Add(nodeFromPull.childs.Keys.Max(), nodeFromPull);
            }
        }

        /// <summary>Find the value in the tree.</summary>
        /// <param name="leaf">The leaf node.</param>
        /// <param name="key">The key to find.</param>
        /// <param name="value">The output of the value found.</param>
        /// <returns>True if found the value.</returns>
        private bool FindValue(BPlusTreeNode<K, V> leaf, K key, out V value) {
            return leaf.values.TryGetValue(key, out value);
        }

        /// <summary>Convert the tree to a list.</summary>
        /// <param name="node">The current node.</param>
        /// <param name="list">The list to add the elements.</param>
        private void ConvertList(BPlusTreeNode<K, V> node, ref List<V> list) {
            if(node.childs == null) {
                list.AddRange(node.values.Values);
            } else if (node.childs.Count > 0) {
                for (int i = 0; i < node.childs.Count; i++) {
                    ConvertList(node.childs.Values[i], ref list);
                }
            }
        }

        /// <summary>Update an item in the tree.</summary>
        /// <param name="node">The actual node.</param>
        /// <param name="key">The key of the item.</param>
        /// <param name="value">The new value.</param>
        private void UpdateItem(BPlusTreeNode<K, V> node, K key, V value) {
            if (node.childs == null) {
                for(int i = 0; i < node.values.Count; i++) {
                    if(node.values.Keys[i].CompareTo(key) == 0) {
                        node.values[node.values.Keys[i]] = value;
                    }
                }
            } else if (node.childs.Count > 0) {
                for (int i = 0; i < node.childs.Count; i++) {
                    UpdateItem(node.childs.Values[i], key, value);
                }
            }
        }

    }

}
