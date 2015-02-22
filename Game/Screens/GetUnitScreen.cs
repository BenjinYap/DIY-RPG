using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class GetUnitScreen:SelectionScreen {
        public enum GetMode { Caught, Received, Bought }

        public GetUnitScreen (Unit unit, string getText, bool canRename):base () {
            if (GameFile._units.Count () < Data._unitInfo.max) {
                Clear (ClearType.Both);

                getText = getText.Replace ("@", unit.name);
                TypeWrite (getText, ConsoleColor.DarkYellow, true);
                string renameQuestion = Data._unitInfo.renameQuestion;
                renameQuestion = renameQuestion.Replace ("@", unit.name);
                TypeWrite (renameQuestion, ConsoleColor.DarkYellow, true);
                
                _selectIndex = 0;
                AddSelections (new List <string> {"Yes", "No"}, 0, true);
                DrawSelections ();
                GetSelection (true);
                Clear (ClearType.AboveDialog);
                ConsoleKey key = ConsoleKey.Applications;

                if (_selectIndex == 0) {
                    Clear (ClearType.AboveDialog);

                    while (true) {
                        string newNameBeforeText = Data._unitInfo.newNameBeforeText;
                        newNameBeforeText = newNameBeforeText.Replace ("@", unit.name);
                        TypeWrite (newNameBeforeText, ConsoleColor.DarkYellow, true);
                        Write (Helper.RCenter (unit.name), 4, ConsoleColor.DarkYellow);
                        int spaces = Data._generalInfo.width - Data._unitInfo.nameLength;
                        Write (new string (' ', spaces / 2) + new string ('-', Data._unitInfo.nameLength), 7, ConsoleColor.DarkYellow);
                        Console.SetCursorPosition (spaces / 2, 6);
                        string name = "";

                        while (key != ConsoleKey.Enter) {
                            key = Console.ReadKey (true).Key;
                            
                            if (name.Length == 0 && key == ConsoleKey.Enter) {
                                key = ConsoleKey.EraseEndOfFile;
                            }

                            if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains (key.ToString ()) && Console.CursorLeft < spaces / 2 + Data._unitInfo.nameLength) {
                                if (Console.CursorLeft == spaces / 2) {
                                    Console.Write (key.ToString ());
                                    name += key.ToString ();
                                } else {
                                    Console.Write (key.ToString ().ToLower ());
                                    name += key.ToString ().ToLower ();
                                }
                            } else if (key == ConsoleKey.Backspace && Console.CursorLeft > spaces / 2) {
                                Console.CursorLeft = Console.CursorLeft - 1;
                                Console.Write (" ");
                                Console.CursorLeft = Console.CursorLeft - 1;
                                name = name.Substring (0, name.Length - 1);
                            }
                        }

                        string newNameAfterText = Data._unitInfo.newNameAfterText;
                        newNameAfterText = newNameAfterText.Replace ("@", unit.name);
                        newNameAfterText = newNameAfterText.Replace ("#", name);
                        TypeWrite (newNameAfterText, ConsoleColor.DarkYellow, true);
                        unit.name = name;
                        break;
                    }
                }

                GameFile._units.Add (unit);
            }

            Clear (ClearType.Both);
        }
    }
}
