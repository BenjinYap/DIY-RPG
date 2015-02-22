using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;

namespace DIY_RPG {

    public class Screen {

        public Screen () {
            
        }

        public void Write (string s, int line, ConsoleColor color) {
            Console.ForegroundColor = color;
            Console.SetCursorPosition (0, line);
            Console.Write (s);
        }

        public void Write (List <string> ss, int firstLine, ConsoleColor color) {
            for (int i = 0; i < ss.Count (); i++) {
                Write (ss [i], firstLine + i, color);
            }
        }

        public void TypeWrite (string rawS, ConsoleColor color, bool pressToContinue, bool instant, bool waitToFinish) {
            if (rawS != null) {
                if (rawS.Length > 0) {
                    int currentLine = Data._generalInfo.dividerLine + 1;
                    Console.SetCursorPosition (0, currentLine);
                    Clear (ClearType.Dialog);

                    string s = rawS;
                    Console.ForegroundColor = color;
                    Console.SetCursorPosition (0, currentLine);
                    bool keyPressed = false;
                    string currentString = "";
                    
                    while (s.Length > 0) {
                        string nextWord = Regex.Match (s, "\\S+").ToString ();
                        
                        if (Console.CursorLeft + nextWord.Length > Data._generalInfo.width - 1) {
                            currentString = "";
                            Console.SetCursorPosition (0, Console.CursorTop + 1);
                            currentLine++;
                            
                            if (s [0] == ' ') {
                                s = s.Substring (1);
                            }
                        }

                        keyPressed = (Console.KeyAvailable) ? true : false;
                        Console.Write (s [0]);
                        currentString += s [0];
                        
                        s = s.Substring (1);
                        
                        if (!keyPressed && !instant) {
                            Thread.Sleep (30);
                        }
                    }

                    if (waitToFinish) {
                        if (keyPressed) {
                            Console.ReadKey (true);
                        }

                        if (pressToContinue) {
                            Console.ReadKey (true);
                        }
                    }
                }
            }
        }

        public void TypeWrite (string rawS, ConsoleColor color, bool pressToContinue) {
            TypeWrite (rawS, color, pressToContinue, false, true);
        }

        protected void Clear (ClearType type) {
            int start = 0, end = 0;

            switch (type) {
                case ClearType.AboveDialog:
                    start = 2;
                    end = Data._generalInfo.dividerLine;
                    break;
                case ClearType.Dialog:
                    start = Data._generalInfo.dividerLine + 1;
                    end = Console.BufferHeight;
                    break;
                case ClearType.Both:
                    start = 2;
                    end = Console.BufferHeight;
                    break;
            }
            
            Clear (start, end);
        }

        protected void Clear (int start, int end) {
            for (int i = start; i < end; i++) {
                Console.SetCursorPosition (0, i);
                int w = (i == end - 1 && end == Console.BufferHeight) ? Console.BufferWidth - 1 : Console.BufferWidth;
                Console.Write (new string (' ', w));

                if (i < Data._generalInfo.dividerLine) {
                    Console.SetCursorPosition (Data._generalInfo.width, i);
                    Console.Write (" ");
                }
            }

            if (start < Data._generalInfo.dividerLine && end > Data._generalInfo.dividerLine) {
                Console.SetCursorPosition (0, Data._generalInfo.dividerLine);
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write (new string ('-', Console.BufferWidth));
            }
        }

        protected void DrawScreenTitle (string title) {
            Write (new string (' ', Data._generalInfo.width), 1, ConsoleColor.DarkYellow);
            Write ("  " + Helper.RCenter (title), 1, ConsoleColor.DarkYellow);
        }
    }
}
