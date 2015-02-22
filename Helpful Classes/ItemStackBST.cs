using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {
    public class ItemStackBST {
        public enum CompareCharResult { FirstLess, SecondLess, Same }

        private class TreeNode {
            public TreeNode leftChild, rightChild;
            public ItemStack data;

            public TreeNode (ItemStack n) {
                data = n;
            }

            public bool Find (ItemStack n) {
                bool found = false;

                if (data.Equals (n)) {
                    found = true;
                } else {
                    if (n.amount <= data.amount) {
                        if (leftChild != null) {
                            found = leftChild.Find (n);
                        }
                    } else {
                        if (rightChild != null) {
                            found = rightChild.Find (n);
                        }
                    }
                }

                return found;
            }

            public List <ItemStack> GetData () {
                List <ItemStack> ss = new List <ItemStack> {};

                if (leftChild != null) {
                    ss.AddRange (leftChild.GetData ());
                }

                ss.Add (data);

                if (rightChild != null) {
                    ss.AddRange (rightChild.GetData ());
                }

                return ss;
            }
        }

        private TreeNode _root;

        public ItemStackBST (List <ItemStack> stacks) {
            for (int i = 0; i < stacks.Count (); i++) {
                Add (stacks [i]);
            }
        }

        private void Add (ItemStack n) {
            if (_root == null) {
                _root = new TreeNode (n);
            } else {
                //if (!Find (n)) {
                    TreeNode node = _root;

                    while (true) {
                        if (n.amount <= node.data.amount) {
                            if (node.leftChild == null) {
                                node.leftChild = new TreeNode (n);
                                break;
                            } else {
                                node = node.leftChild;
                            }
                        } else {
                            if (node.rightChild == null) {
                                node.rightChild = new TreeNode (n);
                                break;
                            } else {
                                node = node.rightChild;
                            }
                        }
                    }
                //}
            }
        }

        public bool Find (ItemStack n) {
            bool found = false;

            if (_root != null) {
                if (_root.data.Equals (n)) {
                    found = true;
                } else {
                    if (n.amount <= _root.data.amount) {
                        if (_root.leftChild != null) {
                            found = _root.leftChild.Find (n);
                        }
                    } else {
                        if (_root.rightChild != null) {
                            found = _root.rightChild.Find (n);
                        }
                    }
                }
            }

            return found;
        }

        public List <ItemStack> GetItemStacks () {
            return _root.GetData ().ToList ();
        }
    }
}