using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {
        
    public class SelectionScreen:Screen {
        private enum Scrolled { Up, Down }

        private List <List <string>> _selections = new List <List <string>> {};
        protected List <int> _selectionLines = new List <int> {};
        public List <bool> _selectionAbles = new List <bool> {};
        protected int _selectIndex;
        protected int _topLine = 3;
        protected int _lowestLine;
        private Scrolled _scrolled = Scrolled.Down;
        protected int _choiceIndex;
        protected List <string> _choices = new List <string> {};

        public SelectionScreen ():base () {

        }

        public void DeleteSelections () {
            _selections = new List <List <string>> {};
            _selectionLines = new List <int> {};
            _selectionAbles = new List <bool> {};
        }

        public void AddSelections (List <string> selections, int space, bool allEnabled) {
            for (int i = 0; i < selections.Count (); i++) {
				Debug.WriteLine (selections [i]);
                AddSelection (selections [i], space, allEnabled);
            }
        }


        private void AddSelectionP (List <string> selection, int space, bool enabled) {
            int line = _topLine;
            
            if (_selections.Count () > 0) {
                line = _lowestLine + 1 + space;
                _lowestLine = line + (selection.Count () - 1);
            } else {
                _lowestLine = _topLine + (selection.Count () - 1);
            }

            for (int i = 0; i < selection.Count (); i++) {
                selection [i] = "  " + selection [i];
            }

            _selections.Add (selection);
            _selectionLines.Add (line);
            _selectionAbles.Add (enabled);
        }

        public void AddSelection (string selection, int space, bool enabled) {
            AddSelectionP (Helper.WrapText (selection, Data._generalInfo.width, true), space, enabled);
        }

        public void AddSelection (List <string> selection, int space, bool enabled) {
            AddSelectionP (selection, space, enabled);
        }

        public virtual void DrawSelections () {
            Clear (_topLine, Data._generalInfo.dividerLine);
            
            for (int i = 0; i < _selections.Count (); i++) {
                if (_selectionLines [i] >= _topLine) {// && _selectionLines [i] + (_selections [i].Count () - 1) <= Data._generalInfo.dividerLine - 1) {
                    List <string> selection = _selections [i];

                    for (int j = 0; j < selection.Count (); j++) {
                        ConsoleColor color = ConsoleColor.Black;
                        
                        if (i == _selectIndex) {
                            if (_selectionAbles [i]) {
                                color = Data._generalInfo.currentSelectionColor;
                            } else {
                                color = Data._generalInfo.disabledSelectionColor;
                            }
                        } else {
                            if (_selectionAbles [i]) {
                                color = Data._generalInfo.selectionColor;
                            } else {
                                color = Data._generalInfo.disabledSelectionColor;
                            }
                        }
                        
                        if (_selectionLines [i] + j < Data._generalInfo.dividerLine) {
                            Write (selection [j], _selectionLines [i] + j, color);
                        }
                    }
                }
            }
            
            Console.SetCursorPosition (0, _topLine);
            Console.ForegroundColor = Data._generalInfo.selectionColor;

            if (_selectionLines [0] < _topLine) {
                Console.Write ("↑");
                Console.CursorLeft = Console.WindowWidth - 1;
                Console.Write ("↑");
            } else {
                Console.Write (" ");
                Console.CursorLeft = Console.WindowWidth - 1;
                Console.Write (" ");
            }

            Console.SetCursorPosition (0, Data._generalInfo.dividerLine - 1);

            if (_selectionLines [_selectionLines.Count () - 1] + (_selections [_selectionLines.Count () - 1].Count () - 1) > Data._generalInfo.dividerLine - 1) {
                Console.Write ("↓");
                Console.CursorLeft = Console.WindowWidth - 1;
                Console.Write ("↓");
            } else {
                Console.Write (" ");
                Console.CursorLeft = Console.WindowWidth - 1;
                Console.Write (" ");
            }
        }

        public int GetSelection (bool canEscape) {
            return GetSelection (canEscape, 0, new List <string> {});
        }

        public int GetSelection (bool canEscape, int selectIndex) {
            return GetSelection (canEscape, selectIndex, new List <string> {});
        }

        public int GetSelection (bool canEscape, List <string> choices) {
            return GetSelection (canEscape, 0, choices);
        }

        public int GetSelection (bool canEscape, int selectIndex, List <string> choices) {
            _selectIndex = selectIndex;
            _choices = choices;
            ConsoleKey key = ConsoleKey.Applications;

            if (!_selectionAbles.Exists (a => a == true)) {
                _scrolled = Scrolled.Up;
                _selectIndex = 0;
                ChangedSelection ();
                _selectIndex = -1;
                DrawSelections ();
                NoEnabledSelections ();

                while (key != ConsoleKey.Escape) {
                    key = Console.ReadKey (true).Key;
                   
                    if (key != ConsoleKey.Escape) {
                        OtherKeyPressed (key);
                    }
                }
            } else {
                if (!_selectionAbles [_selectIndex]) {
                    do {
                        _scrolled = Scrolled.Down;
                        _selectIndex++;

                        if (_selectIndex > _selections.Count () - 1) {
                            _selectIndex = _selections.Count () - 1;
                            break;
                        }
                    } while (!_selectionAbles [_selectIndex]);

                    if (!_selectionAbles [_selectIndex]) {
                        do {
                            _scrolled = Scrolled.Up;
                            _selectIndex--;

                            if (_selectIndex < 0) {
                                _selectIndex = 0;
                                break;
                            }
                        } while (!_selectionAbles [_selectIndex]);
                    }
                }
            

                key = ConsoleKey.Applications;
                bool selected = false;

                while (!selected) {
                    DrawSelections ();
                    ChangedSelection ();
                    PreGetSelection ();

                    while (key != ConsoleKey.Enter) {
                        key = Console.ReadKey (true).Key;
                        
                        if (key == ConsoleKey.W) {
                            int oldIndex = _selectIndex;

                            do {
                                _selectIndex--;

                                if (_selectIndex < 0) {
                                    _selectIndex = 0;
                                    break;
                                }
                            } while (!_selectionAbles [_selectIndex]);

                            if (!_selectionAbles [_selectIndex]) {
                                do {
                                    _selectIndex++;

                                    if (_selectIndex > _selections.Count () - 1) {
                                        _selectIndex = _selections.Count () - 1;
                                        break;
                                    }
                                } while (!_selectionAbles [_selectIndex]);
                            }

                            if (oldIndex != _selectIndex) {
                                _scrolled = Scrolled.Up;
                                ChangedSelection ();
                            }
                        } else if (key == ConsoleKey.S) {
                            int oldIndex = _selectIndex;
                            
                            do {
                                _selectIndex++;

                                if (_selectIndex > _selections.Count () - 1) {
                                    _selectIndex = _selections.Count () - 1;
                                    break;
                                }
                            } while (!_selectionAbles [_selectIndex]);

                            if (!_selectionAbles [_selectIndex]) {
                                do {
                                    _selectIndex--;

                                    if (_selectIndex == -1) {
                                        _selectIndex = 0;
                                        break;
                                    }
                                } while (!_selectionAbles [_selectIndex]);
                            }

                            if (oldIndex != _selectIndex) {
                                _scrolled = Scrolled.Down;
                                ChangedSelection ();
                            }
                        } else if (key == ConsoleKey.Escape && canEscape) {
                            _selectIndex = -1;
                        } else if (key != ConsoleKey.Enter) {
                            OtherKeyPressed (key);
                        }                    

                        if (_selectIndex == -1) {
                            break;
                        }

                        if (!_selectionAbles [_selectIndex]) {
                            key = ConsoleKey.Applications;
                        }
                    }
                    
                    if (_selectIndex == -1) {
                        break;
                    }
                    
                    if (_choices.Count () > 0) {
                        GetSelectionSubChoice ();
                      
                        if (_choiceIndex == 0) {
                            selected = true;
                        } else {
                            key = ConsoleKey.Applications;
                            Clear (ClearType.AboveDialog);
                            //Clear (_topLine, Data._generalInfo.dividerLine);
                        }
                        
                        _choiceIndex = -1;
                    } else {
                        selected = true;
                    }
                }
            }
            
            return _selectIndex;
        }

        protected virtual void NoEnabledSelections () {

        }

        protected virtual void PreGetSelection () {

        }

        protected virtual void OtherKeyPressed (ConsoleKey key) {

        }

        protected virtual void GetSelectionSubChoice () {
            _choiceIndex = 0;
            DrawChoiceBox ();
            GetChoice ();
        }

        private void DrawChoiceBox () {
            int largestChoice = 0;

            for (int i = 0; i < _choices.Count (); i++) {
                if (_choices [i].Length > largestChoice) {
                    largestChoice = _choices [i].Length;
                }
            }

            int choiceBoxTopLine = _selectionLines [_selectIndex];

            if (choiceBoxTopLine + _choices.Count () + 1 >= Data._generalInfo.dividerLine) {
                choiceBoxTopLine = Data._generalInfo.dividerLine - (_choices.Count () + 2);
            }

            Console.SetCursorPosition (Data._generalInfo.width - largestChoice - 2, choiceBoxTopLine);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write ("┌" + new string ('─', largestChoice) + "┐");
            
            for (int i = 0; i < _choices.Count (); i++) {
                Console.SetCursorPosition (Data._generalInfo.width - largestChoice - 2, choiceBoxTopLine + i + 1);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write ("│" + new string (' ', largestChoice) + "│");

                if (i == _choiceIndex) {
                    Console.ForegroundColor = Data._generalInfo.currentSelectionColor;
                } else {
                    Console.ForegroundColor = Data._generalInfo.selectionColor;
                }

                Console.SetCursorPosition (Data._generalInfo.width - largestChoice - 1, choiceBoxTopLine + i + 1);
                Console.Write (_choices [i]);
            }

            Console.SetCursorPosition (Data._generalInfo.width - largestChoice - 2, choiceBoxTopLine + _choices.Count () + 1);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write ("└" + new string ('─', largestChoice) + "┘");
        }

        private void GetChoice () {
            ConsoleKey key = ConsoleKey.Applications;

            while (key != ConsoleKey.Enter && key != ConsoleKey.Escape) {
                key = Console.ReadKey (true).Key;

                if (key == ConsoleKey.W) {
                    _choiceIndex = (_choiceIndex > 0) ? _choiceIndex - 1 : _choiceIndex;
                } else if (key == ConsoleKey.S) {
                    _choiceIndex = (_choiceIndex < _choices.Count () - 1) ? _choiceIndex + 1 : _choiceIndex;
                }

                DrawChoiceBox ();
            }

            if (key == ConsoleKey.Escape) {
                _choiceIndex = -1;
            }
        }

        protected virtual void ChangedSelection () {
            ArrangeLines ();

            DrawSelections ();
        }

        private void ArrangeLines () {
            if (_scrolled == Scrolled.Up) {
                int diff = 0;

                if (_selectionLines [0] < _topLine) {
                    int firstEnabled = _selectionAbles.FindIndex (a => a == true);

                    if (firstEnabled == _selectIndex) {
                        diff = _topLine - _selectionLines [0];
                    } else {
                        if (_selectionLines [_selectIndex] < _topLine) {
                            diff = _topLine - _selectionLines [_selectIndex];
                        }
                    }
                }

                for (int i = 0; i < _selectionLines.Count (); i++) {
                    _selectionLines [i] += diff;
                }
            } else if (_scrolled == Scrolled.Down) {
                int diff = 0;
                
                //if (_selectionLines [_selections.Count () - 1] > Data._generalInfo.dividerLine - 1) {
                if (_selectionLines [_selections.Count () - 1] + (_selections [_selections.Count () - 1].Count () - 1) > Data._generalInfo.dividerLine - 1) {
                    int lastEnabled = _selectionAbles.FindLastIndex (a => a == true);

                    if (lastEnabled == _selectIndex) {
                        diff = _selectionLines [_selections.Count () - 1] + (_selections [_selections.Count () - 1].Count () - 1) - (Data._generalInfo.dividerLine - 1);
                    } else {
                        if (_selectionLines [_selectIndex] + (_selections [_selectIndex].Count () - 1) > Data._generalInfo.dividerLine - 1) {
                            diff = _selectionLines [_selectIndex] + (_selections [_selectIndex].Count () - 1) - (Data._generalInfo.dividerLine - 1);
                        }
                    }

                    int firstVisible = _selectionLines.FindIndex (a => a >= _topLine);

                    if (_selections [firstVisible].Count () > diff && diff > 0) {
                        diff = _selections [firstVisible].Count ();
                    }
                }

                for (int i = 0; i < _selectionLines.Count (); i++) {
                    _selectionLines [i] -= diff;
                }
            }
        }
    }
}
