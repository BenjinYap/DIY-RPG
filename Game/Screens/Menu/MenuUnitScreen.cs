using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class MenuUnitScreen:SelectionScreen {
        private Unit _unit;

        public MenuUnitScreen (Unit unit):base () {
            _unit = unit;
            Clear (ClearType.Both);//┐─└

            AddToSelection (new string ('─', Data._generalInfo.width), false);
            AddToSelection (_unit.name);
            AddToSelection (new string ('─', Data._generalInfo.width), false);
            AddToSelection (Data._unitInfo.statNames ["Level"] + ": " + _unit.level.ToString ());
            AddToSelection (Data._unitInfo.statNames ["HP"] + ": " + ((int) _unit.hpLeft).ToString () + "/" + _unit.stats ["HP"].ToString ());
            AddToSelection (Data._unitInfo.statNames ["Ailment"] + ": " + ((_unit.ailment.turns == 0) ? "None" : _unit.ailment.name));
            AddToSelection (Data._unitInfo.statNames ["EXP"] + ": " + ((int) _unit.exp).ToString () + "/" + _unit.expNext.ToString ());
            AddToSelection (Data._unitInfo.statNames ["Attack"] + ": " + _unit.stats ["Attack"].ToString ());
            AddToSelection (Data._unitInfo.statNames ["SAttack"] + ": " + _unit.stats ["SAttack"].ToString ());
            AddToSelection (Data._unitInfo.statNames ["Defense"] + ": " + _unit.stats ["Defense"].ToString ());
            AddToSelection (Data._unitInfo.statNames ["SDefense"] + ": " + _unit.stats ["SDefense"].ToString ());
            AddToSelection (Data._unitInfo.statNames ["Speed"] + ": " + _unit.stats ["Speed"].ToString ());
            AddToSelection (new string ('─', Data._generalInfo.width), false);

            for (int i = 0; i < _unit.moves.Count (); i++) {
                Move move = _unit.moves [i];
                string usesString = "";

                if (move.uses == -1) {
                    usesString = "∞/∞ uses";
                } else {
                    usesString = move.usesLeft.ToString () + "/" + move.uses.ToString () + " uses";
                }

                string s = move.name + new string (' ', Data._generalInfo.width - move.name.Length - usesString.Length) + usesString;
                AddSelection (new List<string> { s }, 0, true);
            }

            AddToSelection (new string ('─', Data._generalInfo.width), false);
            DrawSelections ();
            int selection = 0;

            while (_selectIndex != -1) {
                selection = GetSelection (true, selection);
            }
        }

        private void AddToSelection (string s, bool enabled) {
            AddSelection (new List <string> {s}, 0, enabled);
        }

        private void AddToSelection (string s) {
            AddToSelection (s, true);
        }

        protected override void ChangedSelection () {
            base.ChangedSelection ();

            if (_selectIndex > 0) {
                if (_selectIndex == 1) {
                    TypeWrite (_unit.desc, ConsoleColor.DarkYellow, false, false, false);
                } else if (_selectIndex >= 13) {
                    TypeWrite (_unit.moves [_selectIndex - 13].desc, ConsoleColor.DarkYellow, false, false, false);
                } else if (_selectIndex >= 4) {
                    TypeWrite (Data._unitInfo.statDescs [Data._unitInfo.statDescs.Keys.ElementAt (_selectIndex - 4)], ConsoleColor.DarkYellow, false, false, false);
                }
            }
        }
    }
}
