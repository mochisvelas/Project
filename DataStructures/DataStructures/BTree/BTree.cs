using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures {

    public class BTree<V, K> where K : IComparable<K> {

        // Properties
        public int Count { get; private set; }
        private BTreeNode<K, V> root;
        private int order;

        /// <summary>Constructor.</summary>
        /// <param name="order">The order of the tree.</param>
        public BTree(int order) {
            Count = 0;
            root = new BTreeNode<K, V>(order);
            this.order = order;
        }

        /// <summary>Insert a new element in the tree.</summary>
        /// <param name="key">The key of the new element.</param>
        /// <param name="value">The value of the new element.</param>
        public void Insert(K key, V value) {
            if (root.IsFull) {
                BTreeNode<K, V> newChilds = root;
                root = new BTreeNode<K, V>(order);
                root.childs.Add(newChilds);
                SplitBTreeNode(root, newChilds, 0);
                InsertElement(root, key, value);
            } else {
                InsertElement(root, key, value);
            }
            Count++;
        }

        /// <summary>Remove an element in the tree.</summary>
        /// <param name="key">The key of the element.</param>
        public void Remove(K key) {
            Delete(root, key);
            if (root.elements.Count == 0 && !root.IsLeaf) {
                root = root.childs.Single();
            }
        }

        /// <summary>Find an element in the tree.</summary>
        /// <param name="key">The key of the element.</param>
        /// <returns>The value of the element.</returns>
        public V Find(K key) {
            Element<K, V> element = Search(root, key);
            if (element == null) {
                return default(V);
            } else {
                return element.value;
            }
        }

        /// <summary>Clear the tree.</summary>
        public void Clear() {
            root = new BTreeNode<K, V>(order);
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
            if (root.elements.Count > 0) {
                Update(root, key, value);
            }
        }

        /// <summary>Check if the tree is empty.</summary>
        /// <returns>True if is empty</returns>
        public bool IsEmpty() {
            if (root.elements.Count == 0)
                return true;
            else
                return false;
        }

        /// <summary>Split the node in two subnodes.</summary>
        /// <param name="parentBTreeNode">The parent node.</param>
        /// <param name="split">The node to split.</param>
        /// <param name="index">The index to split.</param>
        private void SplitBTreeNode(BTreeNode<K, V> parentBTreeNode, BTreeNode<K, V> split, int index) {
            BTreeNode<K, V> newBTreeNode = new BTreeNode<K, V>(order);
            parentBTreeNode.elements.Insert(index, split.elements[order - 1]);
            parentBTreeNode.childs.Insert(index + 1, newBTreeNode);
            newBTreeNode.elements.AddRange(split.elements.GetRange(order, order - 1));
            split.elements.RemoveRange(order - 1, order);
            if (!split.IsLeaf) {
                newBTreeNode.childs.AddRange(split.childs.GetRange(order, order));
                split.childs.RemoveRange(order, order);
            }
        }

        /// <summary>Insert new element in the tree.</summary>
        /// <param name="currentBTreeNode">The current node.</param>
        /// <param name="key">The key of the new element.</param>
        /// <param name="value">The value of the new element.</param>
        private void InsertElement(BTreeNode<K, V> currentBTreeNode, K key, V value) {
            int index = currentBTreeNode.elements.TakeWhile(e => key.CompareTo(e.key) >= 0).Count();
            if (currentBTreeNode.IsLeaf) {
                currentBTreeNode.elements.Insert(index, new Element<K, V>(key, value));
            } else {
                BTreeNode<K, V> child = currentBTreeNode.childs[index];
                if (child.IsFull) {
                    SplitBTreeNode(currentBTreeNode, child, index);
                    if (key.CompareTo(currentBTreeNode.elements[index].key) > 0) {
                        index++;
                    }
                }
                InsertElement(currentBTreeNode.childs[index], key, value);
            }
        }

        /// <summary>Delete one element in the tree.</summary>
        /// <param name="currentBTreeNode">The current node.</param>
        /// <param name="key">The key of the element.</param>
        private void Delete(BTreeNode<K, V> currentBTreeNode, K key) {
            int index = currentBTreeNode.elements.TakeWhile(e => key.CompareTo(e.key) > 0).Count();
            if (index < currentBTreeNode.elements.Count && currentBTreeNode.elements[index].key.CompareTo(key) == 0) {
                DeleteFromBTreeNode(currentBTreeNode, key, index);
            } else {
                if (!currentBTreeNode.IsLeaf) {
                    DeleteFromSub(currentBTreeNode, key, index);
                }
            }
        }

        /// <summary>Deletes an element from a node that contains it, be this node a leaf or an internal node.</summary>
        /// <param name="BTreeNode">The current node.</param>
        /// <param name="key">The key of the element.</param>
        /// <param name="index">The index of the element.</param>
        private void DeleteFromBTreeNode(BTreeNode<K, V> BTreeNode, K key, int index) {
            if (BTreeNode.IsLeaf) {
                BTreeNode.elements.RemoveAt(index);
                Count--;
            } else {
                BTreeNode<K, V> predecessorChild = BTreeNode.childs[index];
                if (predecessorChild.elements.Count >= order) {
                    Element<K, V> predecessor = DeletePredecessor(predecessorChild);
                    BTreeNode.elements[index] = predecessor;
                } else {
                    BTreeNode<K, V> successorChild = BTreeNode.childs[index + 1];
                    if (successorChild.elements.Count >= order) {
                        Element<K, V> successor = DeleteSuccessor(predecessorChild);
                        BTreeNode.elements[index] = successor;
                    } else {
                        predecessorChild.elements.Add(BTreeNode.elements[index]);
                        predecessorChild.elements.AddRange(successorChild.elements);
                        predecessorChild.childs.AddRange(successorChild.childs);
                        BTreeNode.elements.RemoveAt(index);
                        BTreeNode.childs.RemoveAt(index + 1);
                        Delete(predecessorChild, key);
                    }
                }
            }
        }

        /// <summary>Delete an element in a subtree.</summary>
        /// <param name="parentBTreeNode">The parent node.</param>
        /// <param name="key">The key of the element.</param>
        /// <param name="index">The index of the element.</param>
        private void DeleteFromSub(BTreeNode<K, V> parentBTreeNode, K key, int index) {
            BTreeNode<K, V> childBTreeNode = parentBTreeNode.childs[index];
            if (childBTreeNode.HasMinimum) {
                int leftIndex = index - 1;
                BTreeNode<K, V> leftSibling = null;
                if (index > 0) {
                    leftSibling = parentBTreeNode.childs[leftIndex];
                }
                int rightIndex = index + 1;
                BTreeNode<K, V> rightSibling = null;
                if (index < parentBTreeNode.childs.Count - 1) {
                    rightSibling = parentBTreeNode.childs[rightIndex];
                }
                if (leftSibling != null && leftSibling.elements.Count > order - 1) {
                    childBTreeNode.elements.Insert(0, parentBTreeNode.elements[index]);
                    parentBTreeNode.elements[index] = leftSibling.elements.Last();
                    leftSibling.elements.RemoveAt(leftSibling.elements.Count - 1);
                    if (!leftSibling.IsLeaf) {
                        childBTreeNode.childs.Insert(0, leftSibling.childs.Last());
                        leftSibling.childs.RemoveAt(leftSibling.childs.Count - 1);
                    }
                }
                else if (rightSibling != null && rightSibling.elements.Count > order - 1) {
                    childBTreeNode.elements.Add(parentBTreeNode.elements[index]);
                    parentBTreeNode.elements[index] = rightSibling.elements.First();
                    rightSibling.elements.RemoveAt(0);
                    if (!rightSibling.IsLeaf) {
                        childBTreeNode.childs.Add(rightSibling.childs.First());
                        rightSibling.childs.RemoveAt(0);
                    }
                } else {
                    if (leftSibling != null) {
                        childBTreeNode.elements.Insert(0, parentBTreeNode.elements[index]);
                        List<Element<K, V>> oldEntries = childBTreeNode.elements;
                        childBTreeNode.elements = leftSibling.elements;
                        childBTreeNode.elements.AddRange(oldEntries);
                        if (!leftSibling.IsLeaf) {
                            List<BTreeNode<K, V>> oldChildren = childBTreeNode.childs;
                            childBTreeNode.childs = leftSibling.childs;
                            childBTreeNode.childs.AddRange(oldChildren);
                        }
                        parentBTreeNode.childs.RemoveAt(leftIndex);
                        parentBTreeNode.elements.RemoveAt(index);
                    } else {
                        childBTreeNode.elements.Add(parentBTreeNode.elements[index]);
                        childBTreeNode.elements.AddRange(rightSibling.elements);
                        if (!rightSibling.IsLeaf) {
                            childBTreeNode.childs.AddRange(rightSibling.childs);
                        }
                        parentBTreeNode.childs.RemoveAt(rightIndex);
                        parentBTreeNode.elements.RemoveAt(index);
                    }
                }
            }
            Delete(childBTreeNode, key);
        }

        /// <summary>Delete the predecessor of a node.</summary>
        /// <param name="BTreeNode">The current node.</param>
        /// <returns>The oredecessor element.</returns>
        private Element<K, V> DeletePredecessor(BTreeNode<K, V> BTreeNode) {
            if (BTreeNode.IsLeaf) {
                Element<K, V> result = BTreeNode.elements[BTreeNode.elements.Count - 1];
                BTreeNode.elements.RemoveAt(BTreeNode.elements.Count - 1);
                return result;
            }
            return DeletePredecessor(BTreeNode.childs.Last());
        }

        /// <summary>Delete the successor of a node.</summary>
        /// <param name="BTreeNode">The current node.</param>
        /// <returns>The successor element.</returns>
        private Element<K, V> DeleteSuccessor(BTreeNode<K, V> BTreeNode) {
            if (BTreeNode.IsLeaf) {
                Element<K, V> result = BTreeNode.elements[0];
                BTreeNode.elements.RemoveAt(0);
                return result;
            }
            return DeletePredecessor(BTreeNode.childs.First());
        }

        /// <summary>Find an element in the tree.</summary>
        /// <param name="BTreeNode">The current node.</param>
        /// <param name="key">The key of the elemetn.</param>
        /// <returns>The found element.</returns>
        private Element<K, V> Search(BTreeNode<K, V> BTreeNode, K key) {
            int index = BTreeNode.elements.TakeWhile(entry => key.CompareTo(entry.key) > 0).Count();
            if (index < BTreeNode.elements.Count && BTreeNode.elements[index].key.CompareTo(key) == 0) {
                return BTreeNode.elements[index];
            } else {
                if (BTreeNode.IsLeaf) {
                    return null;
                } else {
                    return Search(BTreeNode.childs[index], key);
                }
            }
        }

        /// <summary>Convert the tree to a list.</summary>
        /// <param name="BTreeNode">The current node.</param>
        /// <param name="list">The list to add the elements.</param>
        private void ConvertList(BTreeNode<K, V> BTreeNode, ref List<V> list) {
            if (BTreeNode.elements.Count > 0) {
                if (BTreeNode.childs.Count > 0) {
                    for (int i = 0; i < BTreeNode.childs.Count; i++) {
                        ConvertList(BTreeNode.childs[i], ref list);
                    }
                }
                for (int i = 0; i < BTreeNode.elements.Count; i++) {
                    list.Add(BTreeNode.elements[i].value);
                }
            }
        }

        /// <summary>Update the value of an element in the tree.</summary>
        /// <param name="BTreeNode">The current node.</param>
        /// <param name="key">The key of the element.</param>
        /// <param name="value">The new value of the item.</param>
        private void Update(BTreeNode<K, V> BTreeNode, K key, V value) {
            int index = BTreeNode.elements.TakeWhile(entry => key.CompareTo(entry.key) > 0).Count();
            if (index < BTreeNode.elements.Count && BTreeNode.elements[index].key.CompareTo(key) == 0) {
                BTreeNode.elements[index] = new Element<K, V>(key, value);
            } else {
                if (!BTreeNode.IsLeaf) {
                    Update(BTreeNode.childs[index], key, value);
                }
            }
        }

    }

}
