using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {
    
    public class DeleteMoveScreen:SelectionScreen {
        private Move _newMove;
        private Unit _unit;
        public bool _learned;
        private bool _choosingMove = false;

        public DeleteMoveScreen (Unit unit, Move move):base () {
            _unit = unit;
            _newMove = move;
            DrawScreenTitle ("New Move");
            Clear (ClearType.Both);

            TypeWrite (BuildText (Data._moveInfo.tooManyMovesText, unit, move), ConsoleColor.DarkYellow, true);
            
            AddSelections (new List <string> {"Yes", "No"}, 0, true);
            DrawSelections ();
            GetSelection (true, _selectIndex);
            Clear (ClearType.Dialog);

            if (_selectIndex == 0) {
                _choosingMove = true;
                DeleteSelections ();
                AddSelection (new List <string> {"New Move"}, 0, false);
                AddSelection (new List <string> {_newMove.name}, 0, true);
                _lowestLine++;
                AddSelection (new List <string> {"Existing Moves"}, 0, false);

                for (int i = 0; i < _unit.moves.Count (); i++) {
                    AddSelection (new List <string> {_unit.moves [i].name}, 0, true);
                }

                DrawSelections ();

                do {
                    GetSelection (true, _selectIndex, new List <string> {"Yes", "No"});
                } while (_selectIndex != -1 && _selectIndex == 1);

                if (_selectIndex != -1 && _selectIndex != 0) {
                    Clear (ClearType.AboveDialog);
                    TypeWrite (BuildText (Data._moveInfo.forgotMoveText, unit, _unit.moves [_selectIndex - 3]), ConsoleColor.DarkYellow, true);
                    unit.moves.RemoveAt (_selectIndex - 3);
                    unit.moves.Add (move);
                    TypeWrite (BuildText (Data._moveInfo.learnedMoveText, unit, move), ConsoleColor.DarkYellow, true);
                    _learned = true;
                } else {
                    Clear (ClearType.AboveDialog);
                    TypeWrite (BuildText (Data._moveInfo.didNotLearnMoveText, unit, move), ConsoleColor.DarkYellow, true);
                    _learned = false;
                }
            } else {
                Clear (ClearType.AboveDialog);
                TypeWrite (BuildText (Data._moveInfo.didNotLearnMoveText, unit, move), ConsoleColor.DarkYellow, true);
                _learned = false;
            }
        }

        protected override void ChangedSelection () {
            base.ChangedSelection ();

            if (_choosingMove) {
                if (_selectIndex == 1) {
                    TypeWrite (_newMove.desc, ConsoleColor.DarkYellow, false, false, false);
                } else if (_selectIndex >= 3) {
                    TypeWrite (_unit.moves [_selectIndex - 3].desc, ConsoleColor.DarkYellow, false, false, false);
                }
            }
        }

        private static string BuildText (string s, Unit unit, Move move) {
            s = s.Replace ("@", unit.name);
            s = s.Replace ("#", move.name);
            return s;
        }
    }
}
