using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class MenuBagScreen:SelectionScreen {
        public delegate void ItemEventHandler (Item item, int unitIndex, ref bool usedItem);
        public event ItemEventHandler UseItemEvent;

        private int _catIndex = 0;
        private List <ItemStack> _stacks = new List <ItemStack> {};
        public Item _chosenItem;
        private bool _instant = true;
        public bool _usedItem = false;
        private int _maxWidth = 0;

        public MenuBagScreen ():base () {            
            _topLine = 4;
            DrawScreenTitle (Data._battleInfo.bag);
            Clear (ClearType.Both);

            ChangeCategory ();
        }

        private void ChangeCategory () {
            if (_stacks.Count () <= 0) {
                for (int i = 0; i < Data._itemInfo.categories.Count (); i++) {
                    foreach (ItemStack stack in GameFile._items) {
                        if (stack.category == Data._itemInfo.categories [i]) {
                            _catIndex = i;
                            i = Data._itemInfo.categories.Count ();
                            break;
                        }
                    }
                }
            }

            _selectIndex = 0;
            Clear (ClearType.Both);
            DeleteSelections ();
            _stacks = new List <ItemStack> {};

            foreach (ItemStack stack in GameFile._items) {
                if (stack.category == Data._itemInfo.categories [_catIndex]) {
                    _stacks.Add (stack);
                }
            }

            _stacks = new ItemStackBST (_stacks).GetItemStacks ();
            
            _chosenItem = Data._itemInfo.items.Find (theItem => theItem.id == _stacks [0].itemId);
            Write ("  " + Helper.RCenter ("-" + Data._itemInfo.categories [_catIndex] + "-"), 3, ConsoleColor.DarkYellow);
            _maxWidth = 0;

            for (int i = 0; i < _stacks.Count (); i++) {
                int nameSpace = Data._generalInfo.width - (1 + 1 + 1 + Data._itemInfo.maxStack.ToString ().Length + 1);
                List <string> name = Helper.WrapText (Data._itemInfo.items.Find (item => item.id == _stacks [i].itemId).name, nameSpace, false);

                for (int j = 0; j < name.Count (); j++) {
                    if (name [j].Length + 1 + 1 + 1 + Data._itemInfo.maxStack.ToString ().Length + 1 > _maxWidth) {
                        _maxWidth = name [j].Length + 1 + 1 + 1 + Data._itemInfo.maxStack.ToString ().Length + 1;
                    }
                }
            }

            for (int i = 0; i < _stacks.Count (); i++) {
                int nameSpace = _maxWidth - (1 + 1 + 1 + Data._itemInfo.maxStack.ToString ().Length + 1);
                string space = new string (' ', (Data._generalInfo.width - _maxWidth) / 2);
                ItemStack stack = _stacks [i];
                List <string> name = Helper.WrapText (Data._itemInfo.items.Find (item => item.id == stack.itemId).name, nameSpace, false);
                
                name [0] = space + "│" + name [0] + new string (' ', nameSpace - name [0].Length);
                
                if (name.Count () > 1) {
                    name [0] += "│ " + new string (' ', Data._itemInfo.maxStack.ToString ().Length) + "│";
                    
                    for (int j = 1; j < name.Count (); j++) {
                        if (j == name.Count () - 1) {
                            string amount = (Data._itemInfo.items.Find (item => item.id == stack.itemId).unlimited) ? "1" : stack.amount.ToString ();
                            name [j] += new string (' ', nameSpace - name [j].Length) + "│x" + new string (' ', Data._itemInfo.maxStack.ToString ().Length - amount.Length) + amount + "│";
                        }

                        int wordLength = name [j].Length;
                        name [j] = space + "│" + name [j];

                        if (j < name.Count () - 1) {
                            name [j] += new string (' ', nameSpace - wordLength) + "│ " + new string (' ', Data._itemInfo.maxStack.ToString ().Length) + "│";
                        }
                    }
                } else {
                    string amount = (Data._itemInfo.items.Find (item => item.id == stack.itemId).unlimited) ? "1" : stack.amount.ToString ();
                    name [0] += "│x" + new string (' ', Data._itemInfo.maxStack.ToString ().Length - amount.Length) + amount + "│";
                }

                AddSelection (name, 0, true);
            }

            DrawSelections ();
            ChangedSelection ();
        }

        public void Start () {
            while (_selectIndex != -1) {
                DrawScreenTitle (Data._battleInfo.bag);
                Write ("  " + Helper.RCenter ("-" + Data._itemInfo.categories [_catIndex] + "-"), 3, ConsoleColor.DarkYellow);
                GetSelection (true, _selectIndex);

                if (_selectIndex != -1) {
                    Item item = _chosenItem;
                    bool canUse = true;
                    bool useSuccessful = (item.accuracy >= PreGameScreen._rand.NextDouble ()) ? true : false;
                    int unitIndex = -1;

                    if ((item.flags.Contains ("!bo") && !GameFile._inBattle) || (!item.flags.Contains ("!b") && GameFile._inBattle)) {
                        canUse = false;
                    }

                    if (canUse) {
                        if (item.flags.Contains ("!p")) {
                            MenuUnitsScreen mps = new MenuUnitsScreen (0, "Select the " + " to use this item on.", "There are no " + " that can benefit from this item.");

                            if (item.effect == Item.Effect.HealHP) {
                                for (int i = 0; i < GameFile._units.Count (); i++) {
                                    mps._selectionAbles [i] = (GameFile._units [i].hpLeft <= 0 || GameFile._units [i].hpLeft == GameFile._units [i].stats ["HP"]) ? false : true;
                                }
                            } else if (item.effect == Item.Effect.Revive) {
                                for (int i = 0; i < GameFile._units.Count (); i++) {
                                    mps._selectionAbles [i] = (GameFile._units [i].hpLeft > 0) ? false : true;
                                }
                            } else if (item.effect == Item.Effect.Cure) {
                                for (int i = 0; i < GameFile._units.Count (); i++) {
                                    mps._selectionAbles [i] = (GameFile._units [i].ailment.turns == 0 || GameFile._units [i].hpLeft == 0 || GameFile._units [i].ailment.id != item.effectNum) ? false : true;
                                }
                            } else if (item.effect == Item.Effect.TeachMove || item.effect == Item.Effect.GiveEXP || item.effect == Item.Effect.GiveLevel) {
                                for (int i = 0; i < GameFile._units.Count (); i++) {
                                    mps._selectionAbles [i] = (GameFile._units [i].hpLeft == 0) ? false : true;

                                    if (GameFile._units [i].level == Data._unitInfo.maxLevel && GameFile._units [i].exp == GameFile._units [i].expNext - 1) {
                                        mps._selectionAbles [i] = false;
                                    }
                                }
                            }

                            unitIndex = mps.GetSelection (true);

                            if (unitIndex != -1) {
                                TypeWrite (GetItemUseText (item.id, unitIndex), ConsoleColor.DarkYellow, true);

                                if (useSuccessful) {
                                    UseItemEvent (item, unitIndex, ref _usedItem);
                                }
                            } else {
                                _usedItem = false;
                            }
                        } else {
                            TypeWrite (GetItemUseText (item.id, unitIndex), ConsoleColor.DarkYellow, true);

                            if (useSuccessful) {
                                UseItemEvent (item, unitIndex, ref _usedItem);
                            }
                        }
                    }
                    
                    if (_usedItem) {
                        if (!item.unlimited) {
                            _stacks [_selectIndex].amount--;
                        }

                        if (_stacks [_selectIndex].amount <= 0) {
                            GameFile._items.Remove (_stacks [_selectIndex]);
                            _stacks.RemoveAt (_selectIndex);
                        }
                        
                        if (useSuccessful) {
                            TypeWrite (GetItemSuccessText (item.id, unitIndex), ConsoleColor.DarkYellow, true);
                        } else {
                            TypeWrite (GetItemFailText (item.id, unitIndex), ConsoleColor.DarkYellow, true);
                        }
                        
                        if (GameFile._inBattle) {
                            _selectIndex = -1;
                        } else {
                            if (GameFile._items.Count () > 0) {
                                ChangeCategory ();
                            } else {
                                _selectIndex = -1;
                                Clear (ClearType.Both);
                                DrawScreenTitle ("Menu");
                                TypeWrite (Data._itemInfo.noItemsText, ConsoleColor.DarkYellow, true, false, true);
                            }
                        }
                    }
                }
            }
        }

        public override void DrawSelections () {
            base.DrawSelections ();
            //Write (Helper.RCenter ("-" + Data._itemInfo.categories [_catIndex] + "-"), 3, ConsoleColor.DarkYellow);

            string space = new string (' ', (Data._generalInfo.width - _maxWidth) / 2);

            if (Data._itemInfo.categories.Count () > 1 && GameFile._items.Exists (stack => stack.category != Data._itemInfo.categories [_catIndex])) {
                Write ("  " + space + "←" + new string (' ', _maxWidth - 2) + "→", 2, ConsoleColor.DarkYellow);
            }
        }

        protected override void ChangedSelection () {
            base.ChangedSelection ();
            DrawScreenTitle (Data._battleInfo.bag);
            _chosenItem = Data._itemInfo.items.Find (item => item.id == _stacks [_selectIndex].itemId);
            TypeWrite (Data._itemInfo.items.Find (item => item.id == _chosenItem.id).desc, ConsoleColor.DarkYellow, false, _instant, false);
            _instant = false;
        }

        protected override void OtherKeyPressed (ConsoleKey key) {
            if (Data._itemInfo.categories.Count () > 1 && GameFile._items.Exists (stack => stack.category != Data._itemInfo.categories [_catIndex])) {
                if (key == ConsoleKey.A) {
                    int i = (_catIndex > 0) ? _catIndex - 1 : Data._itemInfo.categories.Count () - 1;

                    while (i != _catIndex) {
                        if (GameFile._items.Exists (stack => stack.category == Data._itemInfo.categories [i])) {
                            _catIndex = i;
                            break;
                        } else {
                            i = (i > 0) ? i - 1 : Data._itemInfo.categories.Count () - 1;
                        }
                    }
                } else if (key == ConsoleKey.D) {
                    int i = (_catIndex < Data._itemInfo.categories.Count () - 1) ? _catIndex + 1 : 0;

                    while (i != _catIndex) {
                        if (GameFile._items.Exists (stack => stack.category == Data._itemInfo.categories [i])) {
                            _catIndex = i;
                            break;
                        } else {
                            i = (i < Data._itemInfo.categories.Count () - 1) ? i + 1 : 0;
                        }
                    }
                }

                ChangeCategory ();
            }
        }

        private string BuildText (string s, int unitIndex) {
            if (unitIndex != -1) {
                s = s.Replace ("@", GameFile._units [unitIndex].name);
            }

            return s;
        }

        private string GetItemUseText (int itemId, int unitIndex) {
            string s = Data._itemInfo.items.Find (i => i.id == itemId).useText;
            s = BuildText (s, unitIndex);
            return s;
        }

        private string GetItemSuccessText (int itemId, int unitIndex) {
            string s = Data._itemInfo.items.Find (i => i.id == itemId).successText;
            s = BuildText (s, unitIndex);
            return s;
        }

        private string GetItemFailText (int itemId, int unitIndex) {
            string s = Data._itemInfo.items.Find (i => i.id == itemId).failText;
            s = BuildText (s, unitIndex);
            return s;
        }
    }
}
