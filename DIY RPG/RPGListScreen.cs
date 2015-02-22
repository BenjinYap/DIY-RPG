using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace DIY_RPG {

    public class RPGListScreen:SelectionScreen {
        private List <XmlDocument> _xmls = new List <XmlDocument> {};
        private List <string> _descs = new List <string> {};

        public RPGListScreen ():base () {
            while (_selectIndex != -1) {
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
                
                Write ("--" + Helper.RCenterF ("DIY RPG", '-') + "--", 0, ConsoleColor.DarkGreen);
                Write ("  User-Created Role Playing Games", 1, ConsoleColor.DarkYellow);
                Write ("--" + new string ('-', Data._generalInfo.width + 1) + "--", Data._generalInfo.dividerLine, ConsoleColor.DarkGreen);

                Clear (ClearType.Both);
                DeleteSelections ();
                Write ("  " + Helper.RCenter ("Your Downloaded RPGs"), 3, ConsoleColor.DarkYellow);
                _topLine = 5;
                string [] rpgs = Directory.GetFiles (@"RPGs\");

                for (int i = 0; i < rpgs.Count (); i++) {
                    XmlDocument xml = new XmlDocument ();
                    xml.Load (rpgs [i]);
                    _xmls.Add (xml);
                    _descs.Add (xml ["data"]["generalInfo"].Attributes ["desc"].Value);
                    AddSelection (xml ["data"]["generalInfo"].Attributes ["title"].Value, 1, true);
                }
                //new PreGameScreen (_xmls [_selectIndex]);
                GetSelection (true, _selectIndex);
                
                if (_selectIndex != -1) {
                    for (int i = 0; i < Console.BufferHeight; i++) {
                        Console.SetCursorPosition (0, i);
                        Console.Write (new string (' ', Console.BufferWidth));
                    }

                    new PreGameScreen (_xmls [_selectIndex]);
                }
            }
        }

        protected override void ChangedSelection () {
            base.ChangedSelection ();
            TypeWrite (_descs [_selectIndex], ConsoleColor.DarkYellow, false, false, false);
        }
    }
}
