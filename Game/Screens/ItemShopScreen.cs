using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class ItemShopScreen:SelectionScreen {
        private enum ShopTab { Buy, Sell }
        private ItemShop _shop;
        private string _space;                 //The blank space before the vertical line
        private int _largestPrice = 0;         //The longest string length of the price list
        private ShopTab _shopTab = ShopTab.Sell;
        private int _maxWidth = 0;

        public ItemShopScreen (int id):base () {
            _shop = Data._itemShops.Find (s => s.id == id);
            Clear (ClearType.Both);
            DrawScreenTitle (_shop.name);
            
            if (_shop.greeting.Length > 0) {
                TypeWrite (_shop.greeting, ConsoleColor.DarkYellow, false, false, false);
            }

            if (_shop.sellItemIDs.Count () == 0) {
                _shopTab = ShopTab.Buy;
            } else {
                _shopTab = ShopTab.Sell;
            }

            while (_selectIndex != -1) {
                SetUpItemShop ();
                GetSelection (true, _selectIndex);

                if (_selectIndex != -1) {
                    if (_shopTab == ShopTab.Sell) {
                        if (Data._itemInfo.items.Find (item => item.id == _shop.sellItemIDs [_selectIndex]).unlimited) {
                            if (!GameFile._items.Exists (item => item.itemId == _shop.sellItemIDs [_selectIndex])) {
                                Buy (_shop.sellItemIDs [_selectIndex]);
                            } else {
                                TypeWrite (BuildText (_shop.alreadyHaveItemText, Data._itemInfo.items.Find (item => item.id == _shop.sellItemIDs [_selectIndex]), ""), ConsoleColor.DarkYellow, false, false, true);
                            }
                        } else {
                            Buy (_shop.sellItemIDs [_selectIndex]);
                        }
                    } else {
                        Sell (_shop.buyItemIDs [_selectIndex]);
                    }
                }
            }
        }

        private void SetUpItemShop () {
            DeleteSelections ();
            Clear (ClearType.AboveDialog);
            List<string> itemNames = new List<string> { };
            List <int> itemIDs = (_shopTab == ShopTab.Sell) ? new List <int> (_shop.sellItemIDs) : new List <int> (_shop.buyItemIDs);
            List <int> prices = (_shopTab == ShopTab.Sell) ? new List<int> (_shop.sellPrices) : new List<int> (_shop.buyPrices);
            _maxWidth = 0;
            int nameSpace = 0;

            for (int i = 0; i < itemIDs.Count (); i++) {
                itemNames.Add (Data._itemInfo.items.Find (item => item.id == itemIDs [i]).name);

                if (prices [i].ToString ().Length > _largestPrice) {
                    _largestPrice = prices [i].ToString ().Length;
                    _largestPrice = (_largestPrice < 5) ? 5 : _largestPrice;
                }

                nameSpace = Data._generalInfo.width - (1 + 1 + _largestPrice + 1);
                List <string> name = Helper.WrapText (itemNames [i], nameSpace, false);

                for (int j = 0; j < name.Count (); j++) {
                    if (name [j].Length + 1 + 1 + _largestPrice + 1 > _maxWidth) {
                        _maxWidth = name [j].Length + 1 + 1 + _largestPrice + 1;
                    }
                }
            }

            _topLine = 9;
            _space = new string (' ', (Data._generalInfo.width - _maxWidth) / 2);
            nameSpace = _maxWidth - 1 - 1 - _largestPrice - 1;
            Write ("  " + new string (' ', Data._generalInfo.width - Data._itemInfo.money.Length - 1 - 1 - GameFile._money.ToString ().Length) + Data._itemInfo.money + ": " + GameFile._money.ToString (), 5, ConsoleColor.DarkYellow);
            Write ("  " + _space + "┌" + new string ('─', nameSpace) + "┬" + new string ('─', _largestPrice) + "┐", 6, ConsoleColor.DarkYellow);
            Write ("  " + _space + "│Item" + new string (' ', nameSpace - 4) + "│" + ((_largestPrice == 5) ? "Price" : "Price" + new string (' ', _largestPrice - 5)) + "│", 7, ConsoleColor.DarkYellow);
            Write ("  " + _space + "├" + new string ('─', nameSpace) + "┼" + new string ('─', _largestPrice) + "┤", 8, ConsoleColor.DarkYellow);

            for (int i = 0; i < itemIDs.Count (); i++) {
                int price = prices [i];
                List <string> name = Helper.WrapText (itemNames [i], nameSpace, false);

                name [0] = _space + "│" + name [0] + new string (' ', nameSpace - name [0].Length);

                if (name.Count () > 1) {
                    name [0] += "│" + new string (' ', _largestPrice) + "│";

                    for (int j = 1; j < name.Count (); j++) {
                        if (j == name.Count () - 1) {
                            name [j] += new string (' ', nameSpace - name [j].Length) + "│" + new string (' ', _largestPrice - price.ToString ().Length) + price.ToString () + "│";
                        }

                        int wordLength = name [j].Length;
                        name [j] = _space + "│" + name [j];

                        if (j < name.Count () - 1) {
                            name [j] += new string (' ', nameSpace - wordLength) + "│" + new string (' ', _largestPrice) + "│";
                        }
                    }
                } else {
                    name [0] += "│" + new string (' ', _largestPrice - price.ToString ().Length) + price.ToString () + "│";
                }

                AddSelection (name, 0, true);
            }

            if (_selectIndex > _selectionLines.Count () - 1) {
                _selectIndex = _selectionLines.Count () - 1;
            }

            DrawSelections ();
        }

        private void DrawAmountBox (List <string> lines, string amount, int amountLineIndex) {
            int largest = 0;

            foreach (string line in lines) {
                largest = (line.Length > largest) ? line.Length : largest;
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            int topLine = _selectionLines [_selectIndex];
            topLine = (topLine + lines.Count () + 1 >= Data._generalInfo.dividerLine) ? Data._generalInfo.dividerLine - lines.Count () - 2 : _topLine;
            Console.SetCursorPosition (1 + Data._generalInfo.width - 2 - largest, topLine);
            Console.Write ("┌" + new string ('─', largest) + "┐");

            for (int i = 0; i < lines.Count (); i++) {
                Console.SetCursorPosition (1 + Data._generalInfo.width - 2 - largest, topLine + i + 1);
                Console.Write ("│" + lines [i] + "│");
            }

            Console.SetCursorPosition (1 + Data._generalInfo.width - 2 - largest, topLine + lines.Count () + 1);
            Console.Write ("└" + new string ('─', largest) + "┘");
            Console.SetCursorPosition (1 + Data._generalInfo.width - 2 - largest + 13, topLine + 1 + amountLineIndex);
            Console.ForegroundColor = Data._generalInfo.currentSelectionColor;
            Console.Write (amount);
        }

        private string HandleKey (ConsoleKey key, string numToBuy) {
            string nums = "0123456798";

            if (key.ToString ().Length == 2 && key.ToString () [0] == 'D') {
                if (nums.Contains (key.ToString () [1]) && numToBuy.Length < 9) {
                    numToBuy += key.ToString () [1];
                }
            } else if (key == ConsoleKey.Backspace && numToBuy.Length > 0) {
                numToBuy = numToBuy.Substring (0, numToBuy.Length - 1);
            }

            while (numToBuy.IndexOf ("0") == 0) {
                numToBuy = numToBuy.Substring (1);
            }

            if (numToBuy.Replace ("0", "").Length == 0) {
                numToBuy = "0";
            }

            return numToBuy;
        }

        private void Buy (int itemID) {
            ConsoleKey key = ConsoleKey.Applications;
            string numToBuy = "0";

            while (true) {
                DrawSelections ();
                Item item = Data._itemInfo.items.Find (theItem => theItem.id == itemID);
                int itemPrice = _shop.sellPrices [_shop.sellItemIDs.IndexOf (itemID)];
                string totalPrice = (Convert.ToInt64 (numToBuy) * itemPrice).ToString ();
                string line2 = "Total Price" + " " + totalPrice;
                string line1 = "# to Buy   " + " " + new string (' ', totalPrice.Length - numToBuy.Length) + numToBuy;
                DrawAmountBox (new List<string> { line1, line2 }, new string (' ', totalPrice.Length - numToBuy.Length) + numToBuy, 0);
                key = Console.ReadKey (true).Key;
                numToBuy = HandleKey (key, numToBuy);

                if (item.unlimited) {
                    if (numToBuy != "0" && numToBuy != "1") {
                        numToBuy = "1";
                    }
                }
                
                if (key == ConsoleKey.Enter) {
                    if (numToBuy != "0") {
                        if (Convert.ToInt64 (totalPrice) <= GameFile._money) {
                            GameFile._money = (int) ((long) GameFile._money - Convert.ToInt64 (totalPrice));
                            TypeWrite (BuildText (_shop.boughtText, item, numToBuy), ConsoleColor.DarkYellow, true, false, true);

                            for (int i = 0; i < Convert.ToInt64 (numToBuy); i++) {
                                GameFile.AddItem (item.id);
                            }

                            break;
                        } else {
                            TypeWrite (BuildText (_shop.cannotAffordText, item, numToBuy), ConsoleColor.DarkYellow, true, false, true);
                            TypeWrite (item.desc, ConsoleColor.DarkYellow, false, false, false);
                        }
                    }
                } else if (key == ConsoleKey.Escape) {
                    break;              //Pressed escape
                }
            }
        }

        private void Sell (int itemID) {
            ConsoleKey key = ConsoleKey.Applications;
            string numToSell = "0";

            while (true) {
                DrawSelections ();
                Item item = Data._itemInfo.items.Find (theItem => theItem.id == itemID);
                int itemPrice = _shop.buyPrices [_shop.buyItemIDs.IndexOf (itemID)];
                string totalProfit = (Convert.ToInt64 (numToSell) * itemPrice).ToString ();
                int numberOwned = 0;
                List <ItemStack> stacks = GameFile._items.FindAll (stack => stack.itemId == itemID);

                foreach (ItemStack stack in stacks) {
                    numberOwned += stack.amount;
                }

                string line1 = "# You Have" + " " + numberOwned.ToString ();
                string line2 = "# to Sell" + " " + numToSell;
                string line3 = "Total Profit" + " " + totalProfit;
                int largest = (line1.Length >= line3.Length) ? line1.Length : line3.Length;
                line1 = "# You Have" + " " + new string (' ', largest - line1.Length) + numberOwned.ToString ();
                line2 = "# to Sell" + " " + new string (' ', largest - line2.Length) + numToSell;
                line3 = "Total Profit" + " " + new string (' ', largest - line3.Length) + totalProfit;
                DrawAmountBox (new List<string> { line1, line2, line3 }, new string (' ', line2.IndexOf (numToSell) - 12) + numToSell, 1);
                key = Console.ReadKey (true).Key;
                numToSell = HandleKey (key, numToSell);

                if (key == ConsoleKey.Enter) {
                    if (numToSell != "0") {
                        if (Convert.ToInt64 (numToSell) > numberOwned) {
                            TypeWrite (BuildText (_shop.notEnoughText, item, numToSell), ConsoleColor.DarkYellow, true, false, true);
                            TypeWrite (item.desc, ConsoleColor.DarkYellow, false, false, false);
                        } else {
                            GameFile._money += Convert.ToInt32 (totalProfit);
                            TypeWrite (BuildText (_shop.soldText, item, numToSell), ConsoleColor.DarkYellow, true, false, true);
                            int numLeftToSell = Convert.ToInt32 (numToSell);
                            ItemStack smallStack = GameFile._items.Find (stack => stack.itemId == itemID && stack.amount < Data._itemInfo.maxStack);
                            numLeftToSell -= smallStack.amount;
                            GameFile._items.Remove (smallStack);

                            while (numLeftToSell >= Data._itemInfo.maxStack) {
                                numLeftToSell -= Data._itemInfo.maxStack;
                                int i = GameFile._items.FindIndex (stack => stack.itemId == itemID && stack.amount == Data._itemInfo.maxStack);
                                GameFile._items.RemoveAt (i);
                            }

                            GameFile._items.Find (stack => stack.itemId == itemID).amount -= numLeftToSell;
                            break;
                        }
                    }
                } else if (key == ConsoleKey.Escape) {
                    break;              //Pressed escape
                }
            }
        }

        public override void DrawSelections () {
            base.DrawSelections ();
            
            if (_lowestLine < Data._generalInfo.dividerLine - 1) {
                Write ("  " + _space + "└" + new string ('─', _maxWidth - 1 - 1 - _largestPrice - 1) + "┴" + new string ('─', _largestPrice) + "┘", _lowestLine + 1, ConsoleColor.DarkYellow);
            }

            if (_shopTab == ShopTab.Sell) {
                Write ("  " + Helper.RCenter (_shop.sellTabText), 3, ConsoleColor.DarkYellow);
            } else {
                Write ("  " + Helper.RCenter (_shop.buyTabText), 3, ConsoleColor.DarkYellow);
            }

            if (_shop.buyItemIDs.Count () > 0 && _shop.sellItemIDs.Count () > 0) {
                Write ("  " + _space + "←" + new string (' ', _maxWidth - 2) + "→", 2, ConsoleColor.DarkYellow);
            }
        }

        protected override void OtherKeyPressed (ConsoleKey key) {
            if (key == ConsoleKey.A || key == ConsoleKey.D) {
                if (_shop.sellItemIDs.Count () > 0 && _shop.buyItemIDs.Count () > 0) {
                    _shopTab = (_shopTab == ShopTab.Sell) ? ShopTab.Buy : ShopTab.Sell;
                    SetUpItemShop ();
                    ChangedSelection ();
                }
            }
        }

        protected override void ChangedSelection () {
            base.ChangedSelection ();
            TypeWrite (Data._itemInfo.items.Find (item => item.id == _shop.sellItemIDs [_selectIndex]).desc, ConsoleColor.DarkYellow, false, false, false);
        }

        private string BuildText (string s, Item item, string amount) {
            s = s.Replace ("@", item.name);
            s = s.Replace ("#", amount);
            return s;
        }
    }
}
