using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;

namespace DIY_RPG {

    public class GameFile {
        public static List <Unit> _units = new List <Unit> {};
        public static int _roomID = 0;        //The ID of the current room
        public static int _spawnRoomID = 0;   //The ID of the room to respawn in when a battle is lost
        public static List <ItemStack> _items = new List <ItemStack> {};
        public static int _money = 100;
        public static bool _inBattle = false;
        public static bool _lastBattleWon = false;
        public static string _fileName = "";
        public static int _lastGetUnitID;
        public static int _lastGetItemID;
        public static int _lastTeachMoveID;
        public static int _lastTeachUnitID;

        public GameFile () {
            
        }

        public static void AddItem (int itemId) {
            Item item = Data._itemInfo.items.Find (i => i.id == itemId);
            int index = _items.FindIndex (stack => stack.itemId == item.id && stack.amount < Data._itemInfo.maxStack);
            
            if (index != -1) {
                ItemStack st = _items [index];
                st.amount++;
                _items [index] = st;
            } else {
                ItemStack st = new ItemStack ();
                st.itemId = itemId;
                st.category = item.category;
                st.amount = 1;
                _items.Add (st);
            }
        }

        public static void RemoveItem (int itemID) {
            if (_items.Exists (stack => stack.itemId == itemID)) {
                ItemStack stack;

                if (_items.Exists (theStack => theStack.amount < Data._itemInfo.maxStack)) {
                    stack = _items.Find (theStack => theStack.amount < Data._itemInfo.maxStack);
                } else {
                    stack = _items.Find (theStack => theStack.itemId == itemID);
                }

                stack.amount--;

                if (stack.amount == 0) {
                    _items.Remove (stack);
                }
            }
        }

        public static void CreateFile () {
            _roomID = Data._startInfo.roomID;
            _spawnRoomID = Data._startInfo.spawnRoomID;
            _money = Data._startInfo.money;
            
            for (int i = 0; i < Data._startInfo.unitIDs.Count (); i++) {
                _units.Add (UnitMaster.MakeUnit (Data._startInfo.unitIDs [i], Data._startInfo.unitLevels [i]));
            }

            for (int i = 0; i < Data._startInfo.itemIDs.Count (); i++) {
                for (int j = 0; j < Data._startInfo.itemAmounts [i]; j++) {
                    GameFile.AddItem (Data._startInfo.itemIDs [i]);
                }
            }
        }

        public static void LoadFile (XmlDocument file) {
            _fileName = file ["data"].Attributes ["fileName"].Value;

            XmlNode playerInfo = file ["data"]["playerInfo"];
            _roomID = Convert.ToInt32 (playerInfo.Attributes ["roomID"].Value);
            _spawnRoomID = Convert.ToInt32 (playerInfo.Attributes ["spawnRoomID"].Value);
            _money = Convert.ToInt32 (playerInfo.Attributes ["money"].Value);
            _lastBattleWon = Boolean.Parse (playerInfo.Attributes ["lastBattleWon"].Value);
            _lastGetUnitID = Convert.ToInt32 (playerInfo.Attributes ["lastGetUnitID"].Value);
            _lastGetItemID = Convert.ToInt32 (playerInfo.Attributes ["lastGetItemID"].Value);
            _lastTeachMoveID = Convert.ToInt32 (playerInfo.Attributes ["lastTeachMoveID"].Value);
            _lastTeachUnitID = Convert.ToInt32 (playerInfo.Attributes ["lastTeachUnitID"].Value);

            XmlNode unitsInBag = file ["data"]["unitsInBag"];

            foreach (XmlNode unitN in unitsInBag) {
                Unit p = UnitMaster.MakeUnit (Convert.ToInt32 (unitN.Attributes ["id"].Value), Convert.ToInt32 (unitN.Attributes ["level"].Value));
                p.name = unitN.Attributes ["name"].Value;
                p.hpLeft = Convert.ToDecimal (unitN.Attributes ["hpLeft"].Value);
                p.exp = Convert.ToDecimal (unitN.Attributes ["exp"].Value);
                p.moves = new List <Move> ();
                string [] moves = unitN.Attributes ["moves"].Value.Split (new char [] {','});

                foreach (string moveS in moves) {
                    int id = Convert.ToInt32 (moveS.Substring (0, moveS.IndexOf ("-")));
                    int usesLeft = Convert.ToInt32 (moveS.Substring (moveS.IndexOf ("-") + 1));
                    Move move = DataCloner.CloneMove (Data._moveInfo.moves.Find (theMove => theMove.id == id));
                    move.usesLeft = usesLeft;
                    p.moves.Add (move);
                }

                GameFile._units.Add (p);
            }

            XmlNode itemsInBag = file ["data"]["itemsInBag"];

            foreach (XmlNode itemN in itemsInBag) {
                for (int i = 0; i < Convert.ToInt32 (itemN.Attributes ["amount"].Value); i++) {
                    AddItem (Convert.ToInt32 (itemN.Attributes ["id"].Value));
                }
            }
        }

        public static void SaveFile () {
            XmlWriterSettings settings = new XmlWriterSettings ();
            settings.Indent = true;
            settings.IndentChars = "    ";

            string [] dirs = Directory.GetDirectories ("Saves\\", Data._generalInfo.title);
            
            if (dirs.Count () == 0) {
                Directory.CreateDirectory ("Saves\\" + Data._generalInfo.title);
            }

            using (XmlWriter writer = XmlWriter.Create ("Saves\\" + Data._generalInfo.title + "\\" + _fileName + ".xml", settings)) {
                writer.WriteStartElement ("data");
                writer.WriteAttributeString ("fileName", _fileName);

                writer.WriteStartElement ("playerInfo");
                writer.WriteAttributeString ("roomID", _roomID.ToString ());
                writer.WriteAttributeString ("money", _money.ToString ());
                writer.WriteAttributeString ("spawnRoomID", _spawnRoomID.ToString ());
                writer.WriteAttributeString ("lastBattleWon", _lastBattleWon.ToString ());
                writer.WriteAttributeString ("lastGetUnitID", _lastGetUnitID.ToString ());
                writer.WriteAttributeString ("lastGetItemID", _lastGetItemID.ToString ());
                writer.WriteAttributeString ("lastTeachMoveID", _lastTeachMoveID.ToString ());
                writer.WriteAttributeString ("lastTeachUnitID", _lastTeachUnitID.ToString ());
                writer.WriteEndElement ();

                writer.WriteStartElement ("unitsInBag");

                for (int i = 0; i < _units.Count (); i++) {
                    Unit p = _units [i];
                    writer.WriteStartElement ("unit");
                    writer.WriteAttributeString ("id", p.id.ToString ());
                    writer.WriteAttributeString ("name", p.name);
                    writer.WriteAttributeString ("level", p.level.ToString ());
                    writer.WriteAttributeString ("hpLeft", p.hpLeft.ToString ());
                    writer.WriteAttributeString ("exp", p.exp.ToString ());
                    string moves = "";

                    for (int j = 0; j < p.moves.Count (); j++) {
                        moves += "," + p.moves [j].id + "-" + p.moves [j].usesLeft;
                    }

                    moves = moves.Substring (1);
                    writer.WriteAttributeString ("moves", moves);
                    writer.WriteEndElement ();
                }

                writer.WriteEndElement ();

                writer.WriteStartElement ("itemsInBag");

                for (int i = 0; i < _items.Count (); i++) {
                    writer.WriteStartElement ("item");
                    writer.WriteAttributeString ("id", _items [i].itemId.ToString ());
                    writer.WriteAttributeString ("amount", _items [i].amount.ToString ());
                    writer.WriteEndElement ();
                }

                writer.WriteEndElement ();

                writer.WriteEndElement ();
                writer.Flush ();
            }
        }

        public static void ClearAll () {
            _units = new List <Unit> {};
            _roomID = 0;
            _spawnRoomID = 0;
            _items = new List <ItemStack> {};
            _money = 100;
            _inBattle = false;
            _lastBattleWon = false;
            _fileName = "";
        }
    }
}
