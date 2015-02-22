using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class MenuUnitsScreen:SelectionScreen {
        private string _space;
        private string _dialog;
        private string _noneDialog;
        private int _largest = 0;

        public MenuUnitsScreen (int selectIndex, string dialog, string noneDialog):base () {
            DrawScreenTitle (Data._battleInfo.units);
            Clear (ClearType.Both);
            _selectIndex = selectIndex;
            _dialog = dialog;
            _noneDialog = noneDialog;
            
            for (int i = 0; i < GameFile._units.Count (); i++) {
                string firstLine = GameFile._units [i].name + " " + Data._unitInfo.levelShort + " " + GameFile._units [i].level.ToString ();
                string secondLine = ((GameFile._units [i].ailment.turns == 0) ? "   " : GameFile._units [i].ailment.tag) + " " + Data._unitInfo.hpShort + " " + ((int) GameFile._units [i].hpLeft).ToString () + "/" + GameFile._units [i].stats ["HP"];
                
                if (firstLine.Length >= secondLine.Length) {
                    _largest = (firstLine.Length > _largest) ? firstLine.Length : _largest;
                } else {
                    _largest = (secondLine.Length > _largest) ? secondLine.Length : _largest;
                }
                
            }
            
            _space = new string (' ', (Data._generalInfo.width - (_largest + 2)) / 2);
            AddToSelection ();
            DrawSelections ();
        }

        private void AddToSelection () {
            for (int i = 0; i < GameFile._units.Count (); i++) {
                List<string> strings = new List<string> { };
                Unit p = GameFile._units [i];
                string firstLine = GameFile._units [i].name + " " + Data._unitInfo.levelShort + " " + GameFile._units [i].level.ToString ();
                string secondLine = ((GameFile._units [i].ailment.turns == 0) ? "   " : GameFile._units [i].ailment.tag) + " " + Data._unitInfo.hpShort + " " + ((int) GameFile._units [i].hpLeft).ToString () + "/" + GameFile._units [i].stats ["HP"];
                int largest = (firstLine.Length >= secondLine.Length) ? firstLine.Length : secondLine.Length;
                
                if (firstLine.Length >= secondLine.Length) {
                    strings.Add (_space + "│" + p.name + new string (' ', _largest - largest) + " " + Data._unitInfo.levelShort + " " + p.level.ToString () + "│");
                    strings.Add (_space + "│" + ((p.ailment.turns == 0) ? "   " : p.ailment.tag) + new string (' ', (_largest - largest) + (firstLine.Length - secondLine.Length)) + " " + Data._unitInfo.hpShort + " " + ((int) p.hpLeft).ToString () + "/" + p.stats ["HP"].ToString () + "│");
                } else {
                    strings.Add (_space + "│" + p.name + new string (' ', (_largest - largest) + (secondLine.Length - firstLine.Length)) + " " + Data._unitInfo.levelShort + " " + p.level.ToString () + "│");
                    strings.Add (_space + "│" + ((p.ailment.turns == 0) ? "   " : p.ailment.tag) + new string (' ', _largest - largest) + " " + Data._unitInfo.hpShort + " " + ((int) p.hpLeft).ToString () + "/" + p.stats ["HP"].ToString () + "│");
                }
                
                AddSelection (strings, 1, true);
            }
        }

        public MenuUnitsScreen (int selectIndex):this (selectIndex, "", "") {
            
        }

        protected override void NoEnabledSelections () {
            if (_noneDialog.Length > 0) {
                TypeWrite (_noneDialog, ConsoleColor.DarkYellow, false, false, true);
            }
        }

        protected override void PreGetSelection () {
            if (_dialog.Length > 0) {
                TypeWrite (_dialog, ConsoleColor.DarkYellow, false, false, true);
            }
        }

        protected override void GetSelectionSubChoice () {
            base.GetSelectionSubChoice ();
            
            if (_choiceIndex == 1) {
                new MenuUnitScreen (GameFile._units [_selectIndex]);
            } else if (_choiceIndex != -1) {
                if (_choices [_choiceIndex] == "Swap") {
                    int firstIndex = _selectIndex;
                    _selectionAbles [_selectIndex] = false;
                    List <string> oldChoices = _choices;
                    GetSelection (true);
                    Unit temp = GameFile._units [_selectIndex];
                    GameFile._units [_selectIndex] = GameFile._units [firstIndex];
                    GameFile._units [firstIndex] = temp;
                    _selectionAbles [firstIndex] = true;
                    DeleteSelections ();
                    AddToSelection ();
                    DrawSelections ();
                    _choices = oldChoices;
                }
            }
        }
    }
}
