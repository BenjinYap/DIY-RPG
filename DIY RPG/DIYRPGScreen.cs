using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class DIYRPGScreen:SelectionScreen {

        public DIYRPGScreen ():base () {
            while (_selectIndex != 3) {
                Data._generalInfo.width = 34;
                Data._generalInfo.aboveHeight = 9;
                Data._generalInfo.dialogHeight = 4;
                Data._generalInfo.dividerLine = 2 + Data._generalInfo.aboveHeight;
                Data._generalInfo.currentSelectionColor = ConsoleColor.DarkRed;
                Data._generalInfo.selectionColor = ConsoleColor.DarkYellow;
                Data._generalInfo.disabledSelectionColor = ConsoleColor.DarkGray;

                Console.SetWindowSize (Data._generalInfo.width + 2, Data._generalInfo.aboveHeight + Data._generalInfo.dialogHeight + 4);
                Console.SetBufferSize (Data._generalInfo.width + 2, Data._generalInfo.aboveHeight + Data._generalInfo.dialogHeight + 4);
                Console.CursorVisible = false;
                Console.Title = "DIY RPG";
                //new RPGListScreen ();
                Write ("--" + Helper.RCenterF ("DIY RPG", '-') + "--", 0, ConsoleColor.DarkGreen);
                Write ("  User-Created Role Playing Games", 1, ConsoleColor.DarkYellow);
                Write ("--" + new string ('-', Data._generalInfo.width + 1) + "--", Data._generalInfo.dividerLine, ConsoleColor.DarkGreen);
                DeleteSelections ();
                AddSelections (new List<string> { "Select an RPG", "Create an RPG", "About DIY RPG", "Exit" }, 1, true);
                GetSelection (false);

                switch (_selectIndex) {
                    case 0:
                        new RPGListScreen ();
                        break;
                    case 1:

                        break;
                    case 2:

                        break;
                }
            }
        }

        protected override void ChangedSelection () {
            base.ChangedSelection ();

            string dialog = "";

            if (_selectIndex == 0) {
                dialog = "Choose from your list of RPGs to play.";
            } else if (_selectIndex == 1) {
                dialog = "Create your very own RPG using the editor.";
            } else if(_selectIndex == 2) {
                dialog = "What this is about.";
            } else {
                dialog = "Please don't go. :(";
            }

            TypeWrite (dialog, ConsoleColor.DarkYellow, false, false, false);
        }
    }
}
