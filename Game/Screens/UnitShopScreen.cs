using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class UnitShopScreen:SelectionScreen {
        /*private enum ShopTab { Buy, Sell }
        private UnitShop _shop;
        private string _space;                 //The blank space before the vertical line
        private int _largestLevel = 0;         //The longest string length of the level list
        private int _largestPrice = 0;         //The longest string length of the price list
        private int _maxWidth = 0;             //The width of the box including borders
        private int _nameSpace = 0;
        private ShopTab _shopTab = ShopTab.Sell;
        private List <int> _playerCanSell = new List<int> {};    //The indexes in the GameFile of units the player can sell
        */
        public UnitShopScreen (int id):base () {
            /*_shop = Data._unitShops.Find (s => s.id == id);
            Clear (ClearType.Both);
            DrawScreenTitle (_shop.name);

            if (_shop.greeting.Length > 0) {
                TypeWrite (_shop.greeting, ConsoleColor.DarkYellow, false, false, false);
            }

            if (_shop.sellUnits.Count () == 0) {
                _shopTab = ShopTab.Buy;
            } else {
                _shopTab = ShopTab.Sell;
            }

            while (_selectIndex != -1) {
                DeleteSelections ();
                
                if (_shopTab == ShopTab.Sell) {
                    SetUpUnitShopSell ();
                } else {
                    SetUpUnitShopBuy ();
                }

                DrawSelections ();

                if (_shopTab == ShopTab.Sell) {
                    GetSelection (true, _selectIndex, new List <string> {"Buy", "Stats", "Cancel"});
                } else {
                    GetSelection (true, _selectIndex, new List<string> { "Sell", "Stats", "Cancel" });
                }

                if (_selectIndex != -1) {
                    if (_shopTab == ShopTab.Sell) {
                        Buy (_shop.sellUnits [_selectIndex]);
                    } else {
                        Sell (_playerCanSell [_selectIndex]);
                    }
                }
            }
        }

        private void SetUpUnitShopSell () {
            DeleteSelections ();
            List<string> unitNames = new List<string> { };
            _largestLevel = 0;

            for (int i = 0; i < _shop.sellUnits.Count (); i++) {
                unitNames.Add (_shop.sellUnits [i].name);

                if (_shop.sellUnits [i].level.ToString ().Length > _largestLevel) {
                    _largestLevel = _shop.sellUnits [i].level.ToString ().Length;
                }
            }

            _largestLevel = (_largestLevel < Data._unitInfo.statNames ["Level"].Length) ? Data._unitInfo.statNames ["Level"].Length : _largestLevel;
            _largestPrice = 0;
            _nameSpace = 0;
            _maxWidth = 0;

            for (int i = 0; i < _shop.sellPrices.Count (); i++) {
                if (_shop.sellPrices [i].ToString ().Length > _largestPrice) {
                    _largestPrice = _shop.sellPrices [i].ToString ().Length;
                    _largestPrice = (_largestPrice < 5) ? 5 : _largestPrice;
                }

                _nameSpace = Data._generalInfo.width - (1 + 1 + _largestLevel + 1 + _largestPrice + 1);
                List<string> name = Helper.WrapText (unitNames [i], _nameSpace, false);

                for (int j = 0; j < name.Count (); j++) {
                    if (name [j].Length + 1 + 1 + _largestLevel + 1 + _largestPrice + 1 > _maxWidth) {
                        _maxWidth = name [j].Length + 1 + 1 + _largestLevel + 1 + _largestPrice + 1;
                    }
                }
            }

            _topLine = 9;
            _space = new string (' ', (Data._generalInfo.width - _maxWidth) / 2);
            _nameSpace = _maxWidth - (1 + 1 + _largestLevel + 1 + _largestPrice + 1);
            Write ("  " + new string (' ', Data._generalInfo.width - Data._itemInfo.money.Length - 1 - 1 - GameFile._money.ToString ().Length) + Data._itemInfo.money + ": " + GameFile._money.ToString (), 5, ConsoleColor.DarkYellow);
            Write ("  " + _space + "┌" + new string ('─', _nameSpace) + "┬" + new string ('─', _largestLevel) + "┬" + new string ('─', _largestPrice) + "┐", 6, ConsoleColor.DarkYellow);
            Write ("  " + _space + "│" + Data._unitInfo.singular + new string (' ', _nameSpace - Data._unitInfo.singular.Length) + "│" + ((_largestLevel == Data._unitInfo.statNames ["Level"].Length) ? Data._unitInfo.statNames ["Level"] : Data._unitInfo.statNames ["Level"].Length + new string (' ', _largestLevel - Data._unitInfo.statNames ["Level"].Length)) + "│" + ((_largestPrice == 5) ? "Price" : "Price" + new string (' ', _largestPrice - 5)) + "│", 7, ConsoleColor.DarkYellow);
            Write ("  " + _space + "├" + new string ('─', _nameSpace) + "┼" + new string ('─', _largestLevel) + "┼" + new string ('─', _largestPrice) + "┤", 8, ConsoleColor.DarkYellow);

            for (int i = 0; i < _shop.sellUnits.Count (); i++) {
                int level = _shop.sellUnits [i].level;
                int price = _shop.sellPrices [i];
                List<string> name = Helper.WrapText (unitNames [i], _nameSpace, false);

                name [0] = _space + "│" + name [0] + new string (' ', _nameSpace - name [0].Length);

                if (name.Count () > 1) {
                    name [0] += "│" + new string (' ', _largestLevel) + "│" + new string (' ', _largestPrice) + "│";

                    for (int j = 1; j < name.Count (); j++) {
                        if (j == name.Count () - 1) {
                            name [j] += new string (' ', _nameSpace - name [j].Length) + "│" + new string (' ', _largestLevel - level.ToString ().Length) + level.ToString () + "│" + new string (' ', _largestPrice - price.ToString ().Length) + price.ToString () + "│";
                        }

                        int wordLength = name [j].Length;
                        name [j] = _space + "│" + name [j];

                        if (j < name.Count () - 1) {
                            name [j] += new string (' ', _nameSpace - wordLength) + "│" + new string (' ', _largestLevel) + "│" + new string (' ', _largestPrice) + "│";
                        }
                    }
                } else {
                    name [0] += "│" + new string (' ', _largestLevel - level.ToString ().Length) + level.ToString () + "│" + new string (' ', _largestPrice - price.ToString ().Length) + price.ToString () + "│";
                }

                AddSelection (name, 0, true);
            }

            if (_selectIndex > _selectionLines.Count () - 1) {
                _selectIndex = _selectionLines.Count () - 1;
            }

            DrawSelections ();
        }

        private void SetUpUnitShopBuy () {
            DeleteSelections ();

            List<string> unitNames = new List<string> { };
            List <bool> hasUnit = new List<bool> {};
            _largestLevel = 0;

            for (int i = 0; i < _shop.buyUnitIDs.Count (); i++) {
                List <Unit> units = (GameFile._units.FindAll (unit => unit.id == _shop.buyUnitIDs [i]));

                for (int j = 0; j < units.Count (); j++) {
                    unitNames.Add (units [j].name);
                    hasUnit.Add (true);
                    _largestLevel = (GameFile._units [j].level.ToString ().Length > _largestLevel) ? GameFile._units [j].level.ToString ().Length : _largestLevel;
                }

                if (units.Count () == 0) {
                    unitNames.Add (Data._unitInfo.units.Find (unit => unit.id == _shop.buyUnitIDs [i]).name + " - None");
                    hasUnit.Add (false);
                }
            }
            
            _largestLevel = (_largestLevel < Data._unitInfo.statNames ["Level"].Length) ? Data._unitInfo.statNames ["Level"].Length : _largestLevel;
            _largestPrice = 0;
            _maxWidth = 0;
            _nameSpace = 0;

            for (int i = 0; i < _shop.buyPrices.Count (); i++) {
                if (_shop.buyPrices [i].ToString ().Length > _largestPrice) {
                    _largestPrice = _shop.buyPrices [i].ToString ().Length;
                    _largestPrice = (_largestPrice < 5) ? 5 : _largestPrice;
                }
            }

            for (int i = 0; i < unitNames.Count (); i++) {
                _nameSpace = Data._generalInfo.width - (1 + 1 + _largestLevel + 1 + _largestPrice + 1);
                List<string> name = Helper.WrapText (unitNames [i], _nameSpace, false);

                for (int j = 0; j < name.Count (); j++) {
                    if (name [j].Length + 1 + 1 + _largestLevel + 1 + _largestPrice + 1 > _maxWidth) {
                        _maxWidth = name [j].Length + 1 + 1 + _largestLevel + 1 + _largestPrice + 1;
                    }
                }
            }

            _topLine = 9;
            _space = new string (' ', (Data._generalInfo.width - _maxWidth) / 2);
            Write ("  " + new string (' ', Data._generalInfo.width - Data._itemInfo.money.Length - 1 - 1 - GameFile._money.ToString ().Length) + Data._itemInfo.money + ": " + GameFile._money.ToString (), 5, ConsoleColor.DarkYellow);
            Write ("  " + _space + "┌" + new string ('─', _nameSpace) + "┬" + new string ('─', _largestLevel) + "┬" + new string ('─', _largestPrice) + "┐", 6, ConsoleColor.DarkYellow);
            Write ("  " + _space + "│" + Data._unitInfo.singular + new string (' ', _nameSpace - Data._unitInfo.singular.Length) + "│" + ((_largestLevel == Data._unitInfo.statNames ["Level"].Length) ? Data._unitInfo.statNames ["Level"] : Data._unitInfo.statNames ["Level"].Length + new string (' ', _largestLevel - Data._unitInfo.statNames ["Level"].Length)) + "│" + ((_largestPrice == 5) ? "Price" : "Price" + new string (' ', _largestPrice - 5)) + "│", 7, ConsoleColor.DarkYellow);
            Write ("  " + _space + "├" + new string ('─', _nameSpace) + "┼" + new string ('─', _largestLevel) + "┼" + new string ('─', _largestPrice) + "┤", 8, ConsoleColor.DarkYellow);

            for (int i = 0; i < unitNames.Count (); i++) {
                //if (unitNames [i] != _shop.noUnitsText) {
                if (hasUnit [i]) {
                    int level = GameFile._units [_playerCanSell [i]].level;
                    int price = _shop.buyPrices [_shop.buyUnitIDs.FindIndex (id => id == GameFile._units [_playerCanSell [i]].id)];
                    List<string> name = Helper.RCenterWrapped (unitNames [i], Data._generalInfo.width / 2, false);

                    name [0] = _space + "│" + name [0] + new string (' ', Data._generalInfo.width / 2 - name [0].Length);

                    if (name.Count () > 1) {
                        name [0] += "│" + new string (' ', _largestLevel) + "│" + new string (' ', _largestPrice) + "│";

                        for (int j = 1; j < name.Count (); j++) {
                            if (j == name.Count () - 1) {
                                name [j] += new string (' ', Data._generalInfo.width / 2 - name [j].Length) + "│" + new string (' ', _largestLevel - level.ToString ().Length) + level.ToString () + "│" + new string (' ', _largestPrice - price.ToString ().Length) + price.ToString () + "│";
                            }

                            int wordLength = name [j].Length;
                            name [j] = _space + "│" + name [j];

                            if (j < name.Count () - 1) {
                                name [j] += new string (' ', Data._generalInfo.width / 2 - wordLength) + "│" + new string (' ', _largestLevel) + "│" + new string (' ', _largestPrice) + "│";
                            }
                        }
                    } else {
                        name [0] += "│" + new string (' ', _largestLevel - level.ToString ().Length) + level.ToString () + "│" + new string (' ', _largestPrice - price.ToString ().Length) + price.ToString () + "│";
                    }

                    AddSelection (name, 0, true);
                } else {
                    List <string> text = Helper.WrapText (unitNames [0], Data._generalInfo.width / 2 + 1 + _largestLevel + 1 + _largestPrice, false);
                    text [0] = _space + "│" + text [0] + "│";
                    AddSelection (text, 0, false);
                }
            }

            if (_selectIndex > _selectionLines.Count () - 1) {
                _selectIndex = _selectionLines.Count () - 1;
            }

            DrawSelections ();
            TypeWrite (_shop.buyUnitsDesc, ConsoleColor.DarkYellow, false, false, false);
        }

        private void Buy (Unit unit) {    //When the player is buying a unit
            if (GameFile._money >= _shop.sellPrices [_shop.sellUnits.IndexOf (unit)]) {
                if (GameFile._units.Count () < Data._unitInfo.max) {
                    GameFile._money -= _shop.sellPrices [_shop.sellUnits.IndexOf (unit)];
                    new GetUnitScreen (unit, _shop.boughtText, _shop.canRenameUnits);
                } else {
                    TypeWrite (Data._unitInfo.noSpaceText, ConsoleColor.DarkYellow, false);
                }
            } else {
                TypeWrite (BuildText (_shop.cannotAffordText, unit), ConsoleColor.DarkYellow, false);
            }
        }

        private void Sell (int gameFileIndex) {    //When the player is selling a unit
            if (GameFile._units.Count () == Data._unitInfo.min) {  //If player has minimum number of units
                TypeWrite (Data._unitInfo.tooFewUnitsText, ConsoleColor.DarkYellow, true, false, true);
            } else {                                               //If player has more than minimum number of units
                TypeWrite (BuildText (_shop.soldText, GameFile._units [gameFileIndex]), ConsoleColor.DarkYellow, true, false, true);
                GameFile._money += _shop.buyPrices [_shop.buyUnitIDs.FindIndex (id => id == GameFile._units [gameFileIndex].id)];
                GameFile._units.RemoveAt (gameFileIndex);
            }
        }

        public override void DrawSelections () {
            base.DrawSelections ();
            Write ("  " + new string (' ', Data._generalInfo.width - Data._itemInfo.money.Length - 1 - 1 - GameFile._money.ToString ().Length) + Data._itemInfo.money + ": " + GameFile._money.ToString (), 5, ConsoleColor.DarkYellow);
            Write ("  " + _space + "┌" + new string ('─', _nameSpace) + "┬" + new string ('─', _largestLevel) + "┬" + new string ('─', _largestPrice) + "┐", 6, ConsoleColor.DarkYellow);
            //Write ("  " + _space + "│" + Data._unitInfo.singular + new string (' ', _nameSpace - Data._unitInfo.singular.Length) + "│" + ((_largestLevel == Data._unitInfo.statNames ["Level"].Length) ? Data._unitInfo.statNames ["Level"] : Data._unitInfo.statNames ["Level"].Length + new string (' ', _largestLevel - Data._unitInfo.statNames ["Level"].Length)) + "│" + ((_largestPrice == 5) ? "Price" : "Price" + new string (' ', _largestPrice - 5)) + "│", 7, ConsoleColor.DarkYellow);
            Write ("  " + _space + "├" + new string ('─', _nameSpace) + "┼" + new string ('─', _largestLevel) + "┼" + new string ('─', _largestPrice) + "┤", 8, ConsoleColor.DarkYellow);

            if (_lowestLine < Data._generalInfo.dividerLine - 1) {
                Write ("  " + _space + "└" + new string ('─', _nameSpace) + "┴" + new string ('─', _largestLevel) + "┴" + new string ('─', _largestPrice) + "┘", _lowestLine + 1, ConsoleColor.DarkYellow);
            }

            if (_shopTab == ShopTab.Sell) {
                Write ("  " + Helper.RCenter (_shop.sellTabText), 3, ConsoleColor.DarkYellow);
            } else {
                Write ("  " + Helper.RCenter (_shop.buyTabText), 3, ConsoleColor.DarkYellow);
            }

            if (_shop.buyUnitIDs.Count () > 0 && _shop.sellUnits.Count () > 0) {
                Write ("  " + _space + "←" + new string (' ', _maxWidth - 2) + "→", 2, ConsoleColor.DarkYellow);
            }
        }

        protected override void OtherKeyPressed (ConsoleKey key) {
            if (key == ConsoleKey.A || key == ConsoleKey.D) {
                if (_shop.sellUnits.Count () > 0 && _shop.buyUnitIDs.Count () > 0) {
                    _shopTab = (_shopTab == ShopTab.Sell) ? ShopTab.Buy : ShopTab.Sell;
                    
                    if (_shopTab == ShopTab.Sell) {
                        SetUpUnitShopSell ();
                        _choices [0] = "Buy";
                    } else {
                        SetUpUnitShopBuy ();
                        _choices [0] = "Sell";
                    }

                    ChangedSelection ();
                }
            }
        }

        protected override void ChangedSelection () {
            base.ChangedSelection ();

            if (_shopTab == ShopTab.Sell) {
                TypeWrite (_shop.sellUnits [_selectIndex].desc, ConsoleColor.DarkYellow, false, false, false);
            }
        }

        protected override void GetSelectionSubChoice () {
            base.GetSelectionSubChoice ();

            if (_choiceIndex == 0) {

            } else if (_choiceIndex == 1) {
                if (_shopTab == ShopTab.Sell) {
                    new MenuUnitScreen (_shop.sellUnits [_selectIndex]);
                } else {
                    new MenuUnitScreen (GameFile._units [_playerCanSell [_selectIndex]]);
                    TypeWrite (_shop.buyUnitsDesc, ConsoleColor.DarkYellow, false, true, false);
                }
            }
        }

        private string BuildText (string s, Unit unit) {
            s = s.Replace ("@", unit.name);
            return s;
        }*/
        }
    }
}
