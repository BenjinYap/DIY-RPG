using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace DIY_RPG {

    public class MenuScreen:SelectionScreen {

        public MenuScreen ():base () {
            _topLine = 5;
            int selection = 0;
            AddSelections (new List <string> {Data._battleInfo.units, Data._battleInfo.bag, "Save", "About " + Data._generalInfo.title, "Quit"}, 0, true);

            while (selection != -1) {
                Clear (ClearType.Both);
                DrawScreenTitle ("Menu");
                Write ("  " + new string (' ', Data._generalInfo.width - Data._itemInfo.money.Length - 2 - GameFile._money.ToString ().Length) + Data._itemInfo.money + ": " + GameFile._money.ToString (), 3, ConsoleColor.DarkYellow);

                selection = GetSelection (true, selection);
                
                if (selection == 0) {
                    if (GameFile._units.Count () > 0) {
                        int selection2 = 0;
                        
                        while (selection2 != -1) {
                            MenuUnitsScreen mps = new MenuUnitsScreen (0);
                            selection2 = mps.GetSelection (true, selection2, new List <string> {"Select", "Stats", "Swap", "Cancel"});
                            
                            if (selection2 != -1) {
                                new MenuUnitScreen (GameFile._units [selection2]);
                            }
                        }
                    } else {
                        TypeWrite (Data._unitInfo.noUnitsText, ConsoleColor.DarkYellow, true, false, true);
                    }
                } else if (selection == 1) {
                    if (GameFile._items.Count () > 0) {
                        MenuBagScreen mbs = new MenuBagScreen ();
                        mbs.UseItemEvent -= onUseItem;
                        mbs.UseItemEvent += onUseItem;
                        mbs.Start ();
                    } else {
                        TypeWrite (Data._itemInfo.noItemsText, ConsoleColor.DarkYellow, true, false, true);
                    }
                } else if (selection == 2) {
                    if (GameFile._fileName.Length == 0) {
                        Clear (ClearType.Both);
                        Write (Helper.RCenterWrapped ("  " + "Enter a name for your save file."), 3, ConsoleColor.DarkYellow);
                        int spaces = Data._generalInfo.width - 20;
                        Write ("  " + new string (' ', spaces / 2) + new string ('-', 20), 7, ConsoleColor.DarkYellow);
                        Console.SetCursorPosition (spaces / 2 + 2, 6);
                        string name = "";
                        ConsoleKey key = ConsoleKey.Applications;

                        while (key != ConsoleKey.Enter) {
                            key = Console.ReadKey (true).Key;

                            if ((name.Length == 0 || name.Replace (" ", "").Length == 0) && key == ConsoleKey.Enter) {
                                key = ConsoleKey.EraseEndOfFile;
                            }

                            if (key != ConsoleKey.Backspace && Console.CursorLeft < spaces / 2 + 20 + 2) {
                                if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains (key.ToString ())) {
                                    Console.Write (key.ToString ().ToLower ());
                                    name += key.ToString ().ToLower ();
                                } else if ("1234567890".Contains (key.ToString ().Substring (1))) {
                                    Console.Write (key.ToString ().Substring (1));
                                    name += key.ToString ().Substring (1);
                                } else if (key == ConsoleKey.Spacebar) {
                                    Console.Write (" ");
                                    name += " ";
                                }
                            } else if (key == ConsoleKey.Backspace && Console.CursorLeft > spaces / 2 + 2) {
                                Console.CursorLeft = Console.CursorLeft - 1;
                                Console.Write (" ");
                                Console.CursorLeft = Console.CursorLeft - 1;
                                name = name.Substring (0, name.Length - 1);
                            }
                        }

                        GameFile._fileName = name;
                    }

                    GameFile.SaveFile ();
                } else if (selection == 3) {
                    new CreditsScreen ();
                } else if (selection == 4) {
                    GameFile.ClearAll ();
                    GameScreen._quit = true;
                    break;
                }
            }
        }

        private void onUseItem (Item item, int unitIndex, ref bool usedItem) {
            usedItem = true;

            if (item.effect == Item.Effect.HealHP) {
                IncreaseHP (GameFile._units [unitIndex], (int) item.effectNum);
            } else if (item.effect == Item.Effect.Revive) {
                IncreaseHP (GameFile._units [unitIndex], (int) (GameFile._units [unitIndex].stats ["HP"] * item.effectNum));
            } else if (item.effect == Item.Effect.TeachMove) {
                usedItem = UnitMaster.LearnMove (this, GameFile._units [unitIndex], Data._moveInfo.moves.Find (m => m.id == (int) item.effectNum));
            } else if (item.effect == Item.Effect.Cure) {
                GameFile._units [unitIndex].ailment.turns = 0;
                new MenuUnitsScreen (unitIndex);
            } else if (item.effect == Item.Effect.GiveEXP) {
                    decimal expBefore = GameFile._units [unitIndex].exp;
                    UnitMaster.IncreaseEXPEvent -= onIncreaseEXP;
                    UnitMaster.IncreaseEXPEvent += onIncreaseEXP;
                    UnitMaster.LevelUpEvent -= onLevelUp;
                    UnitMaster.LevelUpEvent += onLevelUp;
                    UnitMaster.IncreaseEXP (GameFile._units [unitIndex], (int) item.effectNum, true);

                    if (GameFile._units [unitIndex].exp - expBefore < (decimal) item.effectNum) {
                        TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, GameFile._units [unitIndex]), ConsoleColor.DarkYellow, true, false, true);
                    }
            } else if (item.effect == Item.Effect.GiveLevel) {
                if (GameFile._units [unitIndex].level == Data._unitInfo.maxLevel) {
                    TypeWrite (BuildText (Data._unitInfo.cannotLevelUpText, GameFile._units [unitIndex]), ConsoleColor.DarkYellow, true, false, true);
                    GameFile._units [unitIndex].exp = GameFile._units [unitIndex].expNext - 1;
                } else {
                    UnitMaster.LevelUpEvent -= onLevelUp;
                    UnitMaster.LevelUpEvent += onLevelUp;
                    UnitMaster.LevelUpUnit (GameFile._units [unitIndex]);
                }
            }
        }

        private void IncreaseHP (Unit unit, int amount) {
            UnitMaster.IncreaseHPEvent -= onIncreaseHP;
            UnitMaster.IncreaseHPEvent += onIncreaseHP;
            UnitMaster.IncreaseHP (unit, amount, false);
        }

        private void onIncreaseHP (Unit unit) {
            new MenuUnitsScreen (GameFile._units.IndexOf (unit));
        }

        private void onIncreaseEXP (Unit unit) {

        }

        private void onLevelUp (Unit unit) {
            new MenuUnitsScreen (GameFile._units.IndexOf (unit));
            TypeWrite (BuildText (Data._unitInfo.levelUpText, unit), ConsoleColor.DarkYellow, false);

            if (Console.KeyAvailable) {
                Console.ReadKey (true);
            }

            Console.ReadKey (true);

            Clear (ClearType.AboveDialog);

            for (int i = 0; i < unit.nativeMoveLevels.Count (); i++) {
                if (unit.nativeMoveLevels [i] == unit.level) {
                    UnitMaster.LearnMove (this, unit, unit.nativeMoves [i]);
                    new MenuUnitsScreen (GameFile._units.IndexOf (unit));
                }
            }
        }

        private string BuildText (string s, Unit unit) {
            s = s.Replace ("@", unit.name);
            return s;
        }
    }
}
