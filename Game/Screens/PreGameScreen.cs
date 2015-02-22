using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Xml;

namespace DIY_RPG {

    //Loads the specific game's data and saved files, users can exit to the main menu or create a new save file/load a save file and play the game
    public class PreGameScreen:SelectionScreen {
        public static Random _rand = new Random ();
        private XmlDocument _dataXML;

        public PreGameScreen (XmlDocument xml) {
            _dataXML = xml;
            new Data (xml);
            Console.SetWindowSize (Data._generalInfo.width + 4, Data._generalInfo.aboveHeight + Data._generalInfo.dialogHeight + 4);
            Console.SetBufferSize (Data._generalInfo.width + 4, Data._generalInfo.aboveHeight + Data._generalInfo.dialogHeight + 4);
            Console.Title = Data._generalInfo.title;
            Console.CursorVisible = false;
            
            Clear (ClearType.Both);
            DrawScreenTitle ("Player Files");
            Write ("--" + Helper.RCenterF (Data._generalInfo.title, '-')+ "--", 0, ConsoleColor.DarkGreen);
            Write (new string ('-', Console.WindowWidth), Data._generalInfo.dividerLine, ConsoleColor.DarkGreen);
            
            AddSelection ("About " + Data._generalInfo.title, 0, true);
            AddSelection ("New File", 0, true);            
            AddSelection ("-Saved Files-", 1, false);

            string [] dirs = Directory.GetDirectories ("Saves\\", _dataXML ["data"]["generalInfo"].Attributes ["title"].Value);
            string [] saves = new string [] { };
            
            if (dirs.Count () > 0) {
                saves = Directory.GetFiles ("Saves\\" + _dataXML ["data"] ["generalInfo"].Attributes ["title"].Value + "\\");
            }
            
            List <XmlDocument> saveFiles = new List <XmlDocument> {};

            if (saves.Count () == 0) {
                AddSelection ("No saved files", 1, false);
            } else {
                for (int i = 0; i < saves.Count (); i++) {
                    XmlDocument saveFile = new XmlDocument ();
                    saveFile.Load (saves [i]);
                    AddSelection (saveFile ["data"].Attributes ["fileName"].Value, 1, true);
                }
            }

            int selection = 0;

            while (selection != -1) {
                DrawScreenTitle ("Player Files");
                //selection = GetSelection (true, selection);        //Uncomment this for the release
                selection = 3;             //For quick testing

                if (selection != -1) {
                    if (selection == 0) {
                        new CreditsScreen ();
                    } else {
                        XmlDocument file = new XmlDocument ();

                        if (selection != 1) {
                            file.Load (saves [selection - 3]);
                        }
                        
                        new GameScreen (file);
                    }
                }
            }
        }
    }
}
