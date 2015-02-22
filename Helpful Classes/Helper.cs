using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DIY_RPG {

    public class Helper {

        public static string RCenter (string text) {
            int half = (Data._generalInfo.width - text.Length) / 2;            
            string ns = new string (' ', half) + text + new string (' ', half);
            ns += new string (' ', Data._generalInfo.width - ns.Length);
            return ns;
        }

        public static string RCenterF (string text, char spacer) {
            int half = (Data._generalInfo.width - text.Length) / 2;
            string ns = new string (spacer, half) + text;
            ns += new string (spacer, Data._generalInfo.width - ns.Length);
            return ns;
        }

        public static List <string> RCenterWrapped (string s) {
            return RCenterWrapped (s, Data._generalInfo.width, true);
        }

        public static List <string> RCenterWrapped (string s, int width, bool center) {
            List <string> ss = new List <string> {};

            string rawS = s;
            string line = "";

            while (rawS.Length > width) {
                while (line.Length < width) {
                    string nextWord = Regex.Match (rawS, "\\s*\\S+").ToString ();

                    if ((line + nextWord).Length < width) {
                        line += nextWord;
                        rawS = rawS.Substring (nextWord.Length);
                    } else {
                        while (line [0] == ' ') {
                            line = line.Substring (1);
                        }

                        if (center) {
                            ss.Add (new string (' ', (width - line.Length) / 2) + line);
                        } else {
                            ss.Add (line);
                        }

                        line = "";
                    }

                    if (rawS.Length == 0) {
                        break;
                    }
                }

                rawS = line;

                while (rawS [0] == ' ') {
                    rawS = rawS.Substring (1);
                }
            }

            if (center) {
                ss.Add (new string (' ', (width - rawS.Length) / 2) + rawS);
            } else {
                ss.Add (rawS);
            }
            
            return ss;
        }

        public static List <string> WrapText (string text, int width, bool centered) {
            List <string> lines = new List <string> {};

            if (text.Length > width) {
                List <string> words = text.Split (new char [] {' '}).ToList ();
                string line = words [0];
                words.RemoveAt (0);

                while (words.Count () > 0) {
                    if ((line + " " + words [0]).Length <= width) {
                        line += " " + words [0];
                        words.RemoveAt (0);
                    } else {
                        if (centered) {
                            lines.Add (new string (' ', (width - line.Length) / 2) + line);
                        } else {
                            lines.Add (line);
                        }
                        
                        line = words [0];
                        words.RemoveAt (0);
                    }

                    if (words.Count () == 0) {
                        if (centered) {
                            lines.Add (new string (' ', (width - line.Length) / 2) + line);
                        } else {
                            lines.Add (line);
                        }
                    }
                }
            } else {
                if (centered) {
                    lines.Add (new string (' ', (width - text.Length) / 2) + text);
                } else {
                    lines.Add (text);
                }
            }
            
            return lines;
        }
    }
}
