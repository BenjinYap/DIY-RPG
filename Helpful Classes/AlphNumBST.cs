using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {
    public class AlphNumBST {
        public enum CompareCharResult { FirstLess, SecondLess, Same }

        private class TreeNode {
            public TreeNode leftChild, rightChild;
            public string data;

            public TreeNode (string s) {
                data = s;
            }

            public bool Find (string s) {
                bool found = false;

                if (data.Equals (s)) {
                    found = true;
                } else {
                    if (AlphNumBST.CompareString (s, data) == CompareCharResult.FirstLess || AlphNumBST.CompareString (s, data) == CompareCharResult.Same) {
                        if (leftChild != null) {
                            found = leftChild.Find (s);
                        }
                    } else {
                        if (rightChild != null) {
                            found = rightChild.Find (s);
                        }
                    }
                }

                return found;
            }

            public List <string> GetData () {
                List <string> ss = new List <string> {};

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

        public AlphNumBST (List <string> ss) {
            for (int i = 0; i < ss.Count (); i++) {
                Add (ss [i]);
            }
        }

        private void Add (string s) {
            if (_root == null) {
                _root = new TreeNode (s);
            } else {
                if (!Find (s)) {
                    TreeNode node = _root;

                    while (true) {
                        if (AlphNumBST.CompareString (s, node.data) == CompareCharResult.FirstLess || AlphNumBST.CompareString (s, node.data) == CompareCharResult.Same) {
                            if (node.leftChild == null) {
                                node.leftChild = new TreeNode (s);
                                break;
                            } else {
                                node = node.leftChild;
                            }
                        } else {
                            if (node.rightChild == null) {
                                node.rightChild = new TreeNode (s);
                                break;
                            } else {
                                node = node.rightChild;
                            }
                        }
                    }
                }
            }
        }

        public bool Find (string s) {
            bool found = false;

            if (_root != null) {
                if (_root.data.Equals (s)) {
                    found = true;
                } else {
                    if (AlphNumBST.CompareString (s, _root.data) == CompareCharResult.FirstLess || AlphNumBST.CompareString (s, _root.data) == CompareCharResult.Same) {
                        if (_root.leftChild != null) {
                            found = _root.leftChild.Find (s);
                        }
                    } else {
                        if (_root.rightChild != null) {
                            found = _root.rightChild.Find (s);
                        }
                    }
                }
            }

            return found;
        }

        public List <string> GetStrings () {
            return _root.GetData ().ToList ();
        }

        public static CompareCharResult CompareString (string s1, string s2) {
            CompareCharResult ccr = CompareCharResult.Same;
            int result = String.Compare (s1, s2);

            if (result == -1) {
                ccr = CompareCharResult.FirstLess;
            } else if (result == 0) {
                ccr = CompareCharResult.Same;
            } else {
                ccr = CompareCharResult.SecondLess;
            }
            /*s1 = s1.Replace (" ", "").ToLower ();
            s2 = s2.Replace (" ", "").ToLower ();
            int shortest = (s1.Length < s2.Length) ? s1.Length : s2.Length;
            int index = 0;
            CompareCharResult ccr = CompareCharResult.Same;

            do {
                ccr = AlphNumBST.CompareChar (s1 [index], s2 [index]);
                index++;
            } while (ccr == CompareCharResult.Same && index < shortest);
            
            if (ccr == CompareCharResult.Same) {
                ccr = (s1.Length < s2.Length) ? CompareCharResult.FirstLess : CompareCharResult.SecondLess;
            }*/

            return ccr;
        }

        private static CompareCharResult CompareChar (char c1, char c2) {
            string abc = "0123456789abcdefghijklmnopqrstuvwxyz";

            if (abc.IndexOf (c1) > abc.IndexOf (c2)) {
                return CompareCharResult.SecondLess;
            } else if (abc.IndexOf (c1) < abc.IndexOf (c2)) {
                return CompareCharResult.FirstLess;
            } else {
                return CompareCharResult.Same;
            }
        }
    }
}