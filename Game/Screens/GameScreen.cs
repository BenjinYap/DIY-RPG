using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Xml;

namespace DIY_RPG {

    public class GameScreen:SelectionScreen {
        private int _roomId = 0;
        private string _roomName;
        private int _nextRoomId = -1;
        private GameFile _file = new GameFile ();
        private int _breaksLeft = 0;              //How many times to break out of an option loop left
        public static bool _quit = false;

        public GameScreen (XmlDocument saveFile):base () {
            _quit = false;

            if (saveFile.HasChildNodes) {
                GameFile.LoadFile (saveFile);
            } else {
                GameFile.CreateFile ();
            }

            Move (GameFile._roomID);
        }

        private void Move (int roomId) {
            Clear (ClearType.Dialog);
            _nextRoomId = -1;
            _roomId = roomId;
            GameFile._roomID = _roomId;
            Room room = Data._rooms.Find (r => r.id == roomId);
            _roomName = room.name;
            HandleOptionList (room.options, room.dialog);
        }

        private void HandleOptionList (List <Option> options, string dialog) {
            int lastSelectionIndex = 0;
            bool exitOptionList = false;

            while (!exitOptionList) {
                if (_breaksLeft == 0) {
                    DrawScreenTitle (_roomName);

                    if (dialog.Length > 0) {
                        TypeWrite (dialog, ConsoleColor.DarkYellow, true, false, true);
                    } else {
                        Clear (ClearType.Dialog);
                    }

                    DeleteSelections ();
                    AddSelections (Array.ConvertAll<Option, string> (options.ToArray (), delegate (Option option) { return option.text; }).ToList (), 0, true);
                    DrawSelections ();
                    GetSelection (true, lastSelectionIndex);
                    
                    if (_selectIndex == -1) {
                        new MenuScreen ();
                        Clear (ClearType.Both);
                        DrawScreenTitle (_roomName);
                    } else {
                        lastSelectionIndex = _selectIndex;
                        Clear (ClearType.Both);
                        List <OptionItem> currentOptionItems = new List <OptionItem> {};

                        foreach (OptionItem ifAction in options [_selectIndex].ifActions) {
                            currentOptionItems.Add (ifAction);
                        }

                        for (int i = 0; i < currentOptionItems.Count (); i++) {
                            exitOptionList = HandleOptionItem (currentOptionItems [i], currentOptionItems);

                            if (_breaksLeft > 0) {
                                exitOptionList = true;
                                _breaksLeft--;
                            }

                            if (exitOptionList) {
                                break;
                            }
                        }
                        
                        if (_nextRoomId != -1) {
                            Move (_nextRoomId);
                        }
                    }
                } else {
                    _breaksLeft--;
                }

                if (_quit) {
                    break;
                }
            }
        }

        private bool HandleOptionItem (OptionItem ifAction, List <OptionItem> ifActionList) {
            if (ifAction.GetType () == typeof (Loop)) {
                Loop loop = (Loop) ifAction;

                for (int i = 0; i < loop.repeat; i++) {
                    ifActionList.InsertRange (ifActionList.IndexOf (ifAction) + 1, loop.items);
                }

                return false;
            } else if (ifAction.GetType () == typeof (OptionCondition)) {
                OptionCondition condition = (OptionCondition) ifAction;
                bool isTrue = false;

                switch (condition.comparison) {
                    case OptionCondition.Comparison.MoneyGreaterThan:
                        if (GameFile._money > Convert.ToInt32 (condition.expression)) {
                            isTrue = true;
                        }

                        break;
                    case OptionCondition.Comparison.MoneyLessThan:
                        if (GameFile._money < Convert.ToInt32 (condition.expression)) {
                            isTrue = true;
                        }

                        break;
                    case OptionCondition.Comparison.AllUnitsDead:
                        if (UnitMaster.AllDead ()) {
                            isTrue = true;
                        }

                        break;
                    case OptionCondition.Comparison.LastBattleWon:
                        if (GameFile._lastBattleWon) {
                            isTrue = true;
                        }

                        break;
                    case OptionCondition.Comparison.HasItem:
                        if (GameFile._items.Exists (item => item.itemId == Convert.ToInt32 (condition.expression.Substring (0, condition.expression.IndexOf ("-"))))) {
                            List <ItemStack> stacks = GameFile._items.FindAll (item => item.itemId == Convert.ToInt32 (condition.expression.Substring (0, condition.expression.IndexOf ("-"))));
                            int count = 0;
                            
                            foreach (ItemStack stack in stacks) {
                                count += stack.amount;

                                if (count >= Convert.ToInt32 (condition.expression.Substring (condition.expression.IndexOf ("-") + 1))) {
                                    isTrue = true;
                                    break;
                                }
                            }
                        }

                        break;
                    case OptionCondition.Comparison.HasUnit:
                        if (GameFile._units.Exists (unit => unit.id == Convert.ToInt32 (condition.expression))) {
                            isTrue = true;
                        }

                        break;
                    case OptionCondition.Comparison.HasLevelRange:
                        if (GameFile._units.Exists (unit => unit.level >= Convert.ToInt32 (condition.expression.Substring (0, condition.expression.IndexOf ("-"))) && unit.level <= Convert.ToInt32 (condition.expression.Substring (condition.expression.IndexOf ("-") + 1)))) {
                            isTrue = true;
                        }

                        break;
                }

                if (isTrue) {
                    ifActionList.InsertRange (ifActionList.IndexOf (ifAction) + 1, condition.trueItems);
                } else {
                    ifActionList.InsertRange (ifActionList.IndexOf (ifAction) + 1, condition.falseItems);
                }
                
                return false;
            } else if (ifAction.GetType () == typeof (Action)) {
                Action action = (Action) ifAction;

                switch (action.type) {
                    case Action.Type.GetUnit:
                        Unit unit = UnitMaster.MakeUnit (Convert.ToInt32 (action.vars ["unitID"]), Convert.ToInt32 (action.vars ["unitLevel"]));
                        new GetUnitScreen (unit, action.vars ["text"], Boolean.Parse (action.vars ["canRename"]));
                        return false;
                    case Action.Type.GetItem:
                        for (int i = 0; i < Convert.ToInt32 (action.vars ["amount"]); i++) {
                            GameFile.AddItem (Convert.ToInt32 (action.vars ["itemID"]));
                        }

                        return false;
                    case Action.Type.GetMoney:
                        GameFile._money += Convert.ToInt32 (action.vars ["amount"]);
                        return false;
                    case Action.Type.LoseItem:
                        for (int i = 0; i < Convert.ToInt32 (action.vars ["amount"]); i++) {
                            GameFile.RemoveItem (Convert.ToInt32 (action.vars ["itemID"]));
                        }

                        return false;
                    case Action.Type.LoseMoney:
                        if (GameFile._money <= Convert.ToInt32 (action.vars ["amount"])) {
                            GameFile._money = 0;
                        } else {
                            GameFile._money -= Convert.ToInt32 (action.vars ["amount"]);
                        }

                        return false;
                    case Action.Type.UnitGainEXP:
                        if (GameFile._units.Count () > 0) {
                            if (GameFile._units [0].level == Data._unitInfo.maxLevel && GameFile._units [0].exp == GameFile._units [0].expNext - 1) {
                                TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, GameFile._units [0]), ConsoleColor.DarkYellow, true, false, true);
                                break;
                            } else {
                                decimal expBefore = GameFile._units [0].exp;
                                UnitMaster.IncreaseEXPEvent -= onIncreaseEXP;
                                UnitMaster.IncreaseEXPEvent += onIncreaseEXP;
                                UnitMaster.LevelUpEvent -= onLevelUp;
                                UnitMaster.LevelUpEvent += onLevelUp;

                                if (GameFile._units [0].hpLeft > 0) {
                                    UnitMaster.IncreaseEXP (GameFile._units [0], Convert.ToInt32 (action.vars ["amount"]), true);

                                    if (GameFile._units [0].exp - expBefore < Convert.ToDecimal (action.vars ["amount"])) {
                                        TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, GameFile._units [0]), ConsoleColor.DarkYellow, true, false, true);
                                    }
                                }
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.AllUnitsGainEXP:
                        if (GameFile._units.Count () > 0) {
                            UnitMaster.IncreaseEXPEvent -= onIncreaseEXP;
                            UnitMaster.IncreaseEXPEvent += onIncreaseEXP;
                            UnitMaster.LevelUpEvent -= onLevelUp;
                            UnitMaster.LevelUpEvent += onLevelUp;

                            foreach (Unit u in GameFile._units) {
                                if (u.level == Data._unitInfo.maxLevel && u.exp == u.expNext - 1) {
                                    TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, u), ConsoleColor.DarkYellow, true, false, true);
                                    break;
                            } else {
                                    if (u.hpLeft > 0) {
                                        decimal expBefore = u.exp;
                                        UnitMaster.IncreaseEXP (u, Convert.ToInt32 (action.vars ["amount"]), true);

                                        if (u.exp - expBefore < Convert.ToDecimal (action.vars ["amount"])) {
                                            TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, u), ConsoleColor.DarkYellow, true, false, true);
                                        }
                                    }
                                }
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.UnitLevelUp:
                        if (GameFile._units.Count () > 0) {
                            UnitMaster.LevelUpEvent -= onLevelUp;
                            UnitMaster.LevelUpEvent += onLevelUp;

                            for (int i = 0; i < Convert.ToInt32 (action.vars ["amount"]); i++) {
                                if (GameFile._units [0].level == Data._unitInfo.maxLevel) {
                                    TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, GameFile._units [0]), ConsoleColor.DarkYellow, true, false, true);
                                    GameFile._units [0].exp = GameFile._units [0].expNext - 1;
                                    break;
                                } else {
                                    if (GameFile._units [0].hpLeft > 0) {
                                        UnitMaster.LevelUpUnit (GameFile._units [0]);
                                    }
                                }
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.AllUnitsLevelUp:
                        if (GameFile._units.Count () > 0) {
                            UnitMaster.LevelUpEvent -= onLevelUp;
                            UnitMaster.LevelUpEvent += onLevelUp;

                            foreach (Unit u in GameFile._units) {
                                for (int i = 0; i < Convert.ToInt32 (action.vars ["amount"]); i++) {
                                    if (u.level == Data._unitInfo.maxLevel) {
                                       TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, u), ConsoleColor.DarkYellow, true, false, true);
                                       u.exp = u.expNext - 1;
                                        break;
                                    } else {
                                        if (u.hpLeft > 0) {
                                            UnitMaster.LevelUpUnit (u);
                                        }
                                    }
                                }
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.FightWild:
                        if (Convert.ToDouble (action.vars ["chance"]) >= PreGameScreen._rand.NextDouble ()) {
                            if (GameFile._units.Exists (theUnit => theUnit.hpLeft > 0)) {
                                int [] unitIDs = Array.ConvertAll<string, int> (action.vars ["unitIDs"].Split (new char [] { ',' }), delegate (string s) { return Convert.ToInt32 (s); });
                                int minLevel = Convert.ToInt32 (action.vars ["minLevel"]);
                                int maxLevel = Convert.ToInt32 (action.vars ["maxLevel"]);
                                Unit bad = UnitMaster.MakeUnit (unitIDs [PreGameScreen._rand.Next (0, unitIDs.Count ())], PreGameScreen._rand.Next (minLevel, maxLevel + 1));
                                new BattleScreen (bad, Boolean.Parse (action.vars ["canEscape"]), Boolean.Parse (action.vars ["canCatch"]));
                                DrawScreenTitle (_roomName);
                                return false;
                            } else {
                                TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                                _nextRoomId = _roomId;
                                return true;
                            }
                        }

                        return false;
                    case Action.Type.FightTrainer:
                        if (Convert.ToDouble (action.vars ["chance"]) >= PreGameScreen._rand.NextDouble ()) {
                            if (GameFile._units.Exists (theUnit => theUnit.hpLeft > 0)) {
                                new BattleScreen (Data._trainers.Find (t => t.id == Convert.ToInt32 (action.vars ["trainerID"])).Clone (), Boolean.Parse (action.vars ["canEscape"]), Boolean.Parse (action.vars ["canCatch"]));
                                DrawScreenTitle (_roomName);
                                return false;
                            } else {
                                TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                                _nextRoomId = _roomId;
                                return true;
                            }
                        }

                        return false;
                    case Action.Type.SetUnitHP:
                        if (GameFile._units.Count () > 0) {
                            GameFile._units [0].hpLeft = (decimal) (Convert.ToDouble (action.vars ["percent"]) * GameFile._units [0].stats ["HP"]);
                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.SetAllUnitsHP:
                        if (GameFile._units.Count () > 0) {
                            foreach (Unit p in GameFile._units) {
                                p.hpLeft = (decimal) (Convert.ToDouble (action.vars ["percent"]) * p.stats ["HP"]);
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.ChargeUnitMoves:
                        if (GameFile._units.Count () > 0) {
                            for (int i = 0; i < GameFile._units [0].moves.Count (); i++) {
                                Move move = GameFile._units [0].moves [i];
                                move.usesLeft = move.uses;
                                GameFile._units [0].moves [i] = move;
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.ChargeAllUnitsMoves:
                        if (GameFile._units.Count () > 0) {
                            for (int j = 0; j < GameFile._units.Count (); j++) {
                                for (int i = 0; i < GameFile._units [j].moves.Count (); i++) {
                                    Move move = GameFile._units [j].moves [i];
                                    move.usesLeft = move.uses;
                                    GameFile._units [j].moves [i] = move;
                                }
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.TeachUnitMove:
                        if (GameFile._units.Count () > 0) {
                            UnitMaster.LearnMove (this, GameFile._units [0], Data._moveInfo.moves.Find (m => m.id == Convert.ToInt32 (action.vars ["moveID"])));
                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.TeachAllUnitMove:
                        if (GameFile._units.Count () > 0) {
                            foreach (Unit p in GameFile._units) {
                                UnitMaster.LearnMove (this, p, Data._moveInfo.moves.Find (m => m.id == Convert.ToInt32 (action.vars ["moveID"])));
                            }

                            return false;
                        } else {
                            TypeWrite (Data._battleInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                            return true;
                        }
                    case Action.Type.Text:
                        TypeWrite (action.vars ["text"], ConsoleColor.DarkYellow, true);
                        return false;
                    case Action.Type.Move:
                        _nextRoomId = Convert.ToInt32 (action.vars ["roomID"]);
                        DrawScreenTitle (Data._rooms.Find (r => r.id == _nextRoomId).name);
                        return false;
                    case Action.Type.ItemShop:
                        new ItemShopScreen (Convert.ToInt32 (action.vars ["shopID"]));
                        Clear (ClearType.Both);
                        DrawScreenTitle (_roomName);
                        return false;
                    case Action.Type.SetAsSpawn:
                        GameFile._spawnRoomID = GameFile._roomID;
                        return false;
                    case Action.Type.Options:
                        HandleOptionList (action.options, action.vars ["dialog"]);
                        return false;
                    case Action.Type.Break:
                        _breaksLeft = Convert.ToInt32 (action.vars ["amount"]);
                        return true;
                    case Action.Type.SwapRoomIDs:
                        Room r1 = Data._rooms.Find (r => r.id == Convert.ToInt32 (action.vars ["room1ID"]));
                        Room r2 = Data._rooms.Find (r => r.id == Convert.ToInt32 (action.vars ["room2ID"]));
                        int r1Index = Data._rooms.IndexOf (r1);
                        int r2Index = Data._rooms.IndexOf (r2);
                        int tempID = r1.id;
                        r1.id = r2.id;
                        r2.id = tempID;
                        Data._rooms [r1Index] = r1;
                        Data._rooms [r2Index] = r2;
                        return false;
                }
            }
            

            return true;
        }

        private void onIncreaseEXP (Unit unit) {

        }

        private void onLevelUp (Unit unit) {
            TypeWrite (BuildText (Data._unitInfo.levelUpText, unit), ConsoleColor.DarkYellow, false);

            if (Console.KeyAvailable) {
                Console.ReadKey (true);
            }

            Console.ReadKey (true);

            Clear (ClearType.AboveDialog);

            for (int i = 0; i < unit.nativeMoveLevels.Count (); i++) {
                if (unit.nativeMoveLevels [i] == unit.level) {
                    UnitMaster.LearnMove (this, unit, unit.nativeMoves [i]);
                }
            }
        }

        private string BuildText (string s, Unit unit) {
            s = s.Replace ("@", unit.name);
            return s;
        }
    }
}
