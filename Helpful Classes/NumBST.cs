using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {
    public class NumBST {
        public enum CompareCharResult { FirstLess, SecondLess, Same }

        private class TreeNode {
            public TreeNode leftChild, rightChild;
            public int data;

            public TreeNode (int n) {
                data = n;
            }

            public bool Find (int n) {
                bool found = false;

                if (data.Equals (n)) {
                    found = true;
                } else {
                    if (n <= data) {
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

            public List <int> GetData () {
                List <int> ss = new List <int> {};

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

        public NumBST (List <int> ns) {
            for (int i = 0; i < ns.Count (); i++) {
                Add (ns [i]);
            }
        }

        private void Add (int n) {
            if (_root == null) {
                _root = new TreeNode (n);
            } else {
                //if (!Find (n)) {
                    TreeNode node = _root;

                    while (true) {
                        if (n <= node.data) {
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

        public bool Find (int n) {
            bool found = false;

            if (_root != null) {
                if (_root.data.Equals (n)) {
                    found = true;
                } else {
                    if (n <= _root.data) {
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

        public List <int> GetNums () {
            return _root.GetData ().ToList ();
        }
    }
}