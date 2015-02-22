using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace DIY_RPG {

    public class Data {
        public static GeneralInfo _generalInfo = new GeneralInfo ();
        public static MoveInfo _moveInfo = new MoveInfo ();
        public static UnitInfo _unitInfo = new UnitInfo ();
        public static BattleInfo _battleInfo = new BattleInfo ();
        public static ItemInfo _itemInfo = new ItemInfo ();
        public static StartInfo _startInfo = new StartInfo ();

        public static List <Room> _rooms = new List <Room> {};
        public static List <Ailment> _ailments = new List <Ailment> {};
        public static List <Trainer> _trainers = new List <Trainer> {};
        public static List <ItemShop> _itemShops = new List <ItemShop> {};

        private XmlDocument _xml = new XmlDocument ();

        public Data (XmlDocument xml) {
            _xml = xml;
            GetGeneralInfo ();
            GetMoveInfo ();
            GetUnitInfo ();
            GetBattleInfo ();
            GetItemInfo ();
            GetRooms ();
            GetAilments ();
            GetTrainers ();
            GetItemShops ();
            GetStartInfo ();
        }

        private void GetGeneralInfo () {
            XmlNode node = _xml ["data"]["generalInfo"];
            _generalInfo.title = node.Attributes ["title"].Value;
            _generalInfo.width = Convert.ToInt32 (node.Attributes ["width"].Value);
            _generalInfo.aboveHeight = Convert.ToInt32 (node.Attributes ["aboveHeight"].Value);
            _generalInfo.dialogHeight = Convert.ToInt32 (node.Attributes ["dialogHeight"].Value);
            _generalInfo.dividerLine = 2 + _generalInfo.aboveHeight;
            _generalInfo.selectionColor = (ConsoleColor) Enum.Parse (typeof (ConsoleColor), node.Attributes ["selectionColor"].Value);
            _generalInfo.currentSelectionColor = (ConsoleColor) Enum.Parse (typeof (ConsoleColor), node.Attributes ["currentSelectionColor"].Value);
            _generalInfo.disabledSelectionColor = (ConsoleColor) Enum.Parse (typeof (ConsoleColor), node.Attributes ["disabledSelectionColor"].Value);        
            _generalInfo.creditLines = node.Attributes ["creditLines"].Value.Split (new char [] {'|'}).ToList ();
        }

        private void GetMoveInfo () {
            XmlNode node = _xml ["data"]["moves"];
            _moveInfo.learnedMoveText = node.Attributes ["learnedMoveText"].Value;
            _moveInfo.tooManyMovesText = node.Attributes ["tooManyMovesText"].Value;
            _moveInfo.forgotMoveText = node.Attributes ["forgotMoveText"].Value;
            _moveInfo.didNotLearnMoveText = node.Attributes ["didNotLearnMoveText"].Value;
            _moveInfo.alreadyKnowsMoveText = node.Attributes ["alreadyKnowsMoveText"].Value;

            XmlNodeList moves = node.ChildNodes;
            _moveInfo.moves = new List<Move> {};

            foreach (XmlNode moveN in moves) {
                Move move = new Move ();
                move.id = Convert.ToInt32 (moveN.Attributes ["id"].Value);
                move.name = moveN.Attributes ["name"].Value;
                move.desc = moveN.Attributes ["desc"].Value;
                move.uses = Convert.ToInt32 (moveN.Attributes ["uses"].Value);
                move.usesLeft = move.uses;
                move.ifParts = new List<Move.IfPart> { };

                XmlNodeList parts = moveN.ChildNodes;

                foreach (XmlNode partN in parts) {
                    move.ifParts.Add (MakeIfPart (partN, move));
                }

                _moveInfo.moves.Add (move);
            }
        }

        private Move.IfPart MakeIfPart (XmlNode node, Move move) {
            if (node.Name == "if") {
                Move.MoveCondition condition = new Move.MoveCondition ();
                condition.comparison = (Move.MoveCondition.Comparison) Enum.Parse (typeof (Move.MoveCondition.Comparison), node.Attributes ["comparison"].Value);
                condition.expression = node.Attributes ["expression"].Value;
                condition.trueIfParts = new List<Move.IfPart> { };
                condition.falseIfParts = new List<Move.IfPart> { };

                foreach (XmlNode ifPartN in node ["true"]) {
                    condition.trueIfParts.Add (MakeIfPart (ifPartN, move));
                }

                foreach (XmlNode ifPartN in node ["false"]) {
                    condition.falseIfParts.Add (MakeIfPart (ifPartN, move));
                }

                return condition;
            } else if (node.Name == "part") {
                Move.Part part = new Move.Part ();
                part.move = move;
                part.use = Boolean.Parse (node.Attributes ["use"].Value);
                part.target = (Move.Part.Target) Enum.Parse (typeof (Move.Part.Target), node.Attributes ["target"].Value);
                part.effect = node.Attributes ["effect"].Value;
                part.effectNum = Convert.ToDouble (node.Attributes ["effectNum"].Value);
                part.accuracy = Convert.ToDouble (node.Attributes ["accuracy"].Value);
                part.useText = node.Attributes ["useText"].Value;
                part.successText = node.Attributes ["successText"].Value;
                part.missText = node.Attributes ["missText"].Value;
                part.noEffectText = node.Attributes ["noEffectText"].Value;
                return part;
            }

            return null;
        }

        private void GetUnitInfo () {
            XmlNode node = _xml ["data"]["units"];
            _unitInfo.hpShort = node.Attributes ["hpShort"].Value;
            _unitInfo.expShort = node.Attributes ["expShort"].Value;
            _unitInfo.levelShort = node.Attributes ["levelShort"].Value;
            _unitInfo.statNames = new Dictionary <string, string> {};
            _unitInfo.statDescs = new Dictionary <string, string> {};
            string [] statKeys = new string [] {"Name", "Level", "HP", "Ailment", "EXP", "Attack" ,"SAttack" ,"Defense", "SDefense", "Speed"};
            string [] statNames = node.Attributes ["statNames"].Value.Split (new char [] {'|'});
            string [] statDescs = node.Attributes ["statDescs"].Value.Split (new char [] {'|'});

            for (int i = 0; i < statKeys.Count (); i++) {
                _unitInfo.statNames [statKeys [i]] = statNames [i];

                if (i > 0) {
                    _unitInfo.statDescs [statKeys [i]] = statDescs [i - 1];
                }
            }

            _unitInfo.nameLength = Convert.ToInt32 (node.Attributes ["nameLength"].Value);
            _unitInfo.min = Convert.ToInt32 (node.Attributes ["min"].Value);
            _unitInfo.max = Convert.ToInt32 (node.Attributes ["max"].Value);
            _unitInfo.maxLevel = Convert.ToInt32 (node.Attributes ["maxLevel"].Value);
            _unitInfo.cannotLevelUpText = node.Attributes ["cannotLevelUpText"].Value;
            _unitInfo.tooFewUnitsText = node.Attributes ["tooFewUnitsText"].Value;
            _unitInfo.caughtText = node.Attributes ["caughtText"].Value;
            _unitInfo.noSpaceText = node.Attributes ["noSpaceText"].Value;
            string renameQuestion = node.Attributes ["renameQuestion"].Value;
            _unitInfo.renameQuestion = node.Attributes ["renameQuestion"].Value;
            _unitInfo.newNameBeforeText = node.Attributes ["newNameBeforeText"].Value;
            _unitInfo.newNameAfterText = node.Attributes ["newNameAfterText"].Value;
            _unitInfo.noUnitsText = node.Attributes ["noUnitsText"].Value;
            _unitInfo.gainEXPText = node.Attributes ["gainEXPText"].Value;
            _unitInfo.levelUpText = node.Attributes ["levelUpText"].Value;
            _unitInfo.maxMoves = Convert.ToInt32 (node.Attributes ["maxMoves"].Value);
            _unitInfo.expNextBase = Convert.ToInt32 (node.Attributes ["expNextBase"].Value);
            _unitInfo.expNextMult = Convert.ToDecimal (node.Attributes ["expNextMult"].Value);
            _unitInfo.enemyExpMult = Convert.ToDecimal (node.Attributes ["enemyExpMult"].Value);

            XmlNodeList units = node.ChildNodes;
            _unitInfo.units = new List<Unit> {};

            foreach (XmlNode unitN in units) {
                Unit unit = new Unit ();
                unit.id = Convert.ToInt32 (unitN.Attributes ["id"].Value);
                unit.name = unitN.Attributes ["name"].Value;
                unit.desc = unitN.Attributes ["desc"].Value;
                unit.nativeMoves = new List<Move> { };
                unit.nativeMoveLevels = new List<int> { };
                string [] nativeMoveStrings = unitN.Attributes ["nativeMoves"].Value.Split (new string [] { "," }, StringSplitOptions.None);

                for (int i = 0; i < nativeMoveStrings.Count (); i++) {
                    string s = nativeMoveStrings [i];
                    unit.nativeMoves.Add (Data._moveInfo.moves.Find (m => m.id == Convert.ToInt32 (s.Substring (0, s.IndexOf ("-")))));
                    unit.nativeMoveLevels.Add (Convert.ToInt32 (s.Substring (s.IndexOf ("-") + 1, s.Length - s.IndexOf ("-") - 1)));
                }

                unit.stats = new Dictionary<string, int> { };
                unit.statGrowths = new Dictionary<string, int> { };
                statKeys = new string [] { "HP", "Attack", "SAttack", "Defense", "SDefense", "Speed" };
                string [] baseStatValues = unitN.Attributes ["baseStats"].Value.Split (new string [] { "," }, StringSplitOptions.None);
                string [] statGrowthValues = unitN.Attributes ["statGrowths"].Value.Split (new string [] { "," }, StringSplitOptions.None);

                for (int i = 0; i < statKeys.Count (); i++) {
                    unit.stats [statKeys [i]] = Convert.ToInt32 (baseStatValues [i]);
                    unit.statGrowths [statKeys [i]] = Convert.ToInt32 (statGrowthValues [i]);
                }

                unit.enemyBaseExp = Convert.ToInt32 (unitN.Attributes ["enemyBaseExp"].Value);
                unit.encounterText = unitN.Attributes ["encounterText"].Value;
                unit.diedText = unitN.Attributes ["diedText"].Value;
                unit.playerLoseText = unitN.Attributes ["playerLoseText"].Value;
                unit.playerWinText = unitN.Attributes ["playerWinText"].Value;
                unit.moneyGainText = unitN.Attributes ["moneyGainText"].Value;
                unit.moneyLoseText = unitN.Attributes ["moneyLoseText"].Value;
                unit.moneyGain = Convert.ToInt32 (unitN.Attributes ["moneyGain"].Value);
                unit.moneyLose = Convert.ToInt32 (unitN.Attributes ["moneyLose"].Value);
                _unitInfo.units.Add (unit);
            }
        }

        private void GetBattleInfo () {
            XmlNode node = _xml ["data"]["battleInfo"];
            _battleInfo.noUnitsText = node.Attributes ["noUnitsText"].Value;
            _battleInfo.mainMenuText = node.Attributes ["mainMenuText"].Value;
            _battleInfo.fightMenuText = node.Attributes ["fightMenuText"].Value;
            _battleInfo.fight = node.Attributes ["fight"].Value;
            _battleInfo.bag = node.Attributes ["bag"].Value;
            _battleInfo.units = node.Attributes ["units"].Value;
            _battleInfo.run = node.Attributes ["run"].Value;
            _battleInfo.runChanceLevelDiff = Convert.ToInt32 (node.Attributes ["runChanceLevelDiff"].Value);
            _battleInfo.escapeSuccessText = node.Attributes ["escapeSuccessText"].Value;
            _battleInfo.escapeFailText = node.Attributes ["escapeFailText"].Value;
            _battleInfo.cannotEscapeText = node.Attributes ["cannotEscapeText"].Value;
            _battleInfo.sendOutUnitText = node.Attributes ["sendOutUnitText"].Value;
            _battleInfo.swapUnitText = node.Attributes ["swapUnitText"].Value;
            _battleInfo.cannotSwapUnitText = node.Attributes ["cannotSwapUnitText"].Value;
            _battleInfo.unitReturnText = node.Attributes ["unitReturnText"].Value;
            _battleInfo.replaceUnitText = node.Attributes ["replaceUnitText"].Value;
            _battleInfo.yourUnitDiedText = node.Attributes ["yourUnitDiedText"].Value;
            _battleInfo.allUnitsDeadText = node.Attributes ["allUnitsDeadText"].Value;
            _battleInfo.cannotCatchUnitText = node.Attributes ["cannotCatchUnitText"].Value;
        }

        private void GetItemInfo () {
            XmlNode node = _xml ["data"]["items"];
            _itemInfo.money = node.Attributes ["money"].Value;
            _itemInfo.categories = node.Attributes ["categories"].Value.Split (new char [] {'|'}).ToList ();
            _itemInfo.maxStack = Convert.ToInt32 (node.Attributes ["maxStack"].Value);
            _itemInfo.noItemsText = node.Attributes ["noItemsText"].Value;

            XmlNodeList items = node.ChildNodes;
            _itemInfo.items = new List<Item> {};

            foreach (XmlNode itemN in items) {
                Item item = new Item ();
                item.id = Convert.ToInt32 (itemN.Attributes ["id"].Value);
                item.name = itemN.Attributes ["name"].Value;
                item.desc = itemN.Attributes ["desc"].Value;
                item.category = itemN.Attributes ["category"].Value;
                item.flags = itemN.Attributes ["flags"].Value;
                item.effect = (Item.Effect) Enum.Parse (typeof (Item.Effect), itemN.Attributes ["effect"].Value);
                item.effectNum = Convert.ToDouble (itemN.Attributes ["effectNum"].Value);
                item.accuracy = Convert.ToDouble (itemN.Attributes ["accuracy"].Value);
                item.useText = itemN.Attributes ["useText"].Value;
                item.successText = itemN.Attributes ["successText"].Value;
                item.failText = itemN.Attributes ["failText"].Value;
                item.unlimited = Boolean.Parse (itemN.Attributes ["unlimited"].Value);
                _itemInfo.items.Add (item);
            }
        }

        private void GetRooms () {
            XmlNodeList rooms = _xml ["data"]["rooms"].ChildNodes;
            
            foreach (XmlNode roomN in rooms) {
                XmlNodeList optionsL = roomN.ChildNodes;
                List <Option> options = new List <Option> {};

                foreach (XmlNode optionN in optionsL) {
                    options.Add (MakeOption (optionN));
                }
                
                Room room = new Room ();
                room.id = Convert.ToInt32 (roomN.Attributes ["id"].Value);
                room.name = roomN.Attributes ["name"].Value;
                room.dialog = roomN.Attributes ["dialog"].Value;
                room.options = options;
                _rooms.Add (room);
            }
        }

        private Option MakeOption (XmlNode node) {
            Option option = new Option ();
            option.text = node.Attributes ["text"].Value;
            List <OptionItem> ifActions = new List <OptionItem> {};
            
            foreach (XmlNode ifActionN in node.ChildNodes) {
                ifActions.Add (MakeOptionItem (ifActionN));
            }

            option.ifActions = ifActions;
            return option;
        }

        private OptionItem MakeOptionItem (XmlNode node) {
            if (node.Name == "loop") {
                Loop loop=  new Loop ();
                loop.repeat = Convert.ToInt32 (node.Attributes ["repeat"].Value);
                loop.items = new List<OptionItem> {};

                foreach (XmlNode itemNode in node.ChildNodes) {
                    loop.items.Add (MakeOptionItem (itemNode));
                }

                return loop;
            } else if (node.Name == "if") {
                OptionCondition condition = new OptionCondition ();
                condition.comparison = (OptionCondition.Comparison) Enum.Parse (typeof (OptionCondition.Comparison), node.Attributes ["comparison"].Value);
                condition.expression = node.Attributes ["expression"].Value;
                condition.trueItems = new List <OptionItem> {};
                condition.falseItems = new List <OptionItem> {};

                foreach (XmlNode ifActionN in node ["true"]) {
                    condition.trueItems.Add (MakeOptionItem (ifActionN));
                }

                foreach (XmlNode ifActionN in node ["false"]) {
                    condition.falseItems.Add (MakeOptionItem (ifActionN));
                }

                return condition;
            } else if (node.Name == "action") {
                Action action = new Action ();
                action.type = (Action.Type) Enum.Parse (typeof (Action.Type), node.Attributes ["type"].Value);
                action.vars = new Dictionary <string, string> {};

                foreach (XmlAttribute attribute in node.Attributes) {
                    if (attribute.Name != "type") {
                        action.vars [attribute.Name] = attribute.Value;
                    }
                }

                if (action.type == Action.Type.Options) {
                    action.options = new List <Option> {};

                    foreach (XmlNode optionN in node.ChildNodes) {
                        action.options.Add (MakeOption (optionN));
                    }
                }

                return action;
            }

            return null;
        }

        private void GetAilments () {
            XmlNodeList ailments = _xml ["data"]["ailments"].ChildNodes;

            foreach (XmlNode ailmentN in ailments) {
                Ailment ailment = new Ailment ();
                ailment.id = Convert.ToInt32 (ailmentN.Attributes ["id"].Value);
                ailment.name = ailmentN.Attributes ["name"].Value;
                ailment.type = (Ailment.Type) Enum.Parse (typeof (Ailment.Type), ailmentN.Attributes ["type"].Value);
                ailment.damageType = (Ailment.DamageType) Enum.Parse (typeof (Ailment.DamageType), ailmentN.Attributes ["damageType"].Value);
                ailment.successText = ailmentN.Attributes ["successText"].Value;
                ailment.missText = ailmentN.Attributes ["missText"].Value;
                ailment.noEffectText = ailmentN.Attributes ["noEffectText"].Value;
                ailment.tag = ailmentN.Attributes ["tag"].Value;
                ailment.turns = Convert.ToInt32 (ailmentN.Attributes ["turns"].Value);
                ailment.chance = Convert.ToDouble (ailmentN.Attributes ["chance"].Value);
                _ailments.Add (ailment);
            }
        }

        private void GetTrainers () {
            XmlNodeList trainers = _xml ["data"]["trainers"].ChildNodes;

            foreach (XmlNode trainerN in trainers) {
                Trainer trainer = new Trainer ();
                trainer.id = Convert.ToInt32 (trainerN.Attributes ["id"].Value);
                trainer.name = trainerN.Attributes ["name"].Value;
                trainer.units = new List <Unit> {};
                List <string> idsAndLevels = trainerN.Attributes ["units"].Value.Split (new string [] {","}, StringSplitOptions.None).ToList ();

                foreach (string s in idsAndLevels) {
                    int id = Convert.ToInt32 (s.Substring (0, s.IndexOf ("-")));
                    int level = Convert.ToInt32 (s.Substring (s.IndexOf ("-") + 1));
                    trainer.units.Add (UnitMaster.MakeUnit (id, level));
                }

                trainer.encounterText = trainerN.Attributes ["encounterText"].Value;
                trainer.sendOutUnitText = trainerN.Attributes ["sendOutUnitText"].Value;
                trainer.unitDiedText = trainerN.Attributes ["unitDiedText"].Value;
                trainer.allUnitsDiedText = trainerN.Attributes ["allUnitsDiedText"].Value;
                trainer.playerLoseText = trainerN.Attributes ["playerLoseText"].Value;
                trainer.playerWinText = trainerN.Attributes ["playerWinText"].Value;
                trainer.moneyGainText = trainerN.Attributes ["moneyGainText"].Value;
                trainer.moneyLoseText = trainerN.Attributes ["moneyLoseText"].Value;
                trainer.moneyGain = Convert.ToInt32 (trainerN.Attributes ["moneyGain"].Value);
                trainer.moneyLose = Convert.ToInt32 (trainerN.Attributes ["moneyLose"].Value);
                _trainers.Add (trainer);
            }
        }

        private void GetItemShops () {
            XmlNodeList shops = _xml ["data"]["itemShops"].ChildNodes;

            foreach (XmlNode shopN in shops) {
                ItemShop shop = new ItemShop ();
                shop.id = Convert.ToInt32 (shopN.Attributes ["id"].Value);
                shop.name = shopN.Attributes ["name"].Value;
                shop.greeting = shopN.Attributes ["greeting"].Value;
                shop.sellTabText = shopN.Attributes ["sellTabText"].Value;
                shop.buyTabText = shopN.Attributes ["buyTabText"].Value;
                shop.boughtText = shopN.Attributes ["boughtText"].Value;
                shop.cannotAffordText = shopN.Attributes ["cannotAffordText"].Value;
                shop.alreadyHaveItemText = shopN.Attributes ["alreadyHaveItemText"].Value;
                shop.sellItemIDs = new List <int> {};
                shop.sellPrices = new List <int> {};
                shop.sortMode = (ItemShop.SortMode) Enum.Parse (typeof (ItemShop.SortMode), shopN.Attributes ["sortMode"].Value);
                string [] itemsAndPrices = shopN.Attributes ["sellItems"].Value.Split (new char [] {','});

                if (itemsAndPrices [0].Length > 0) {
                    foreach (string s in itemsAndPrices) {
                        shop.sellItemIDs.Add (Convert.ToInt32 (s.Substring (0, s.IndexOf ("-"))));
                        shop.sellPrices.Add (Convert.ToInt32 (s.Substring (s.IndexOf ("-") + 1)));
                    }

                    if (shop.sortMode == ItemShop.SortMode.ByName) {
                        List <string> sortedItemNames = new List<string> {};

                        for (int i = 0; i < shop.sellItemIDs.Count (); i++) {
                            sortedItemNames.Add (Data._itemInfo.items.Find (theItem => theItem.id == shop.sellItemIDs [i]).name);
                        }

                        List <string> unsortedItemNames = new List <string> (sortedItemNames);
                        sortedItemNames = new AlphNumBST (sortedItemNames).GetStrings ();
                        SortArraysByName (unsortedItemNames, sortedItemNames, new List <List <int>> {shop.sellItemIDs, shop.sellPrices});
                    } else if (shop.sortMode == ItemShop.SortMode.ByPrice) {
                        List<int> unsortedItemPrices = shop.sellPrices;
                        List <int> sortedItemPrices = new NumBST (unsortedItemPrices).GetNums ();
                        SortArraysByNumber (unsortedItemPrices, sortedItemPrices, shop.sellItemIDs, shop.sellPrices);
                    }
                }

                shop.soldText = shopN.Attributes ["soldText"].Value;
                shop.notEnoughText = shopN.Attributes ["notEnoughText"].Value;
                shop.buyItemIDs = new List<int> {};
                shop.buyPrices = new List<int> {};
                itemsAndPrices = shopN.Attributes ["buyItems"].Value.Split (new char [] {','});

                if (itemsAndPrices [0].Length > 0) {
                    foreach (string s in itemsAndPrices) {
                        shop.buyItemIDs.Add (Convert.ToInt32 (s.Substring (0, s.IndexOf ("-"))));
                        shop.buyPrices.Add (Convert.ToInt32 (s.Substring (s.IndexOf ("-") + 1)));
                    }

                    if (shop.sortMode == ItemShop.SortMode.ByName) {
                        List<string> sortedItemNames = new List<string> { };

                        for (int i = 0; i < shop.buyItemIDs.Count (); i++) {
                            sortedItemNames.Add (Data._itemInfo.items.Find (theItem => theItem.id == shop.buyItemIDs [i]).name);
                        }

                        List <string> unsortedItemNames = new List<string> (sortedItemNames);
                        sortedItemNames = new AlphNumBST (sortedItemNames).GetStrings ();
                        SortArraysByName (unsortedItemNames, sortedItemNames, new List <List <int>> {shop.buyItemIDs, shop.buyPrices});
                    } else if (shop.sortMode == ItemShop.SortMode.ByPrice) {
                        List<int> unsortedItemPrices = shop.buyPrices;
                        List<int> sortedItemPrices = new NumBST (unsortedItemPrices).GetNums ();
                        SortArraysByNumber (unsortedItemPrices, sortedItemPrices, shop.buyItemIDs, shop.buyPrices);
                    }
                }

                _itemShops.Add (shop);
            }
        }

        private void SortArraysByName (List <string> unsortedNames, List <string> sortedNames, List <List <int>> arrays) {
            for (int i = 0; i < sortedNames.Count (); i++) {
                int indexInUnsorted = unsortedNames.IndexOf (sortedNames [i]);
                string tempName = unsortedNames [i];
                unsortedNames [i] = unsortedNames [indexInUnsorted];
                unsortedNames [indexInUnsorted] = tempName;
                
                for (int j = 0; j < arrays.Count (); j++) {
                    int temp = arrays [j][i];
                    arrays [j][i] = arrays [j][indexInUnsorted];
                    arrays [j][indexInUnsorted] = temp;
                }
            }
        }

        private void SortArraysByNumber (List <int> unsortedNums, List <int> sortedNums, List <int> array1, List <int> array2) {
            for (int i = 0; i < sortedNums.Count (); i++) {
                int indexInUnsorted = unsortedNums.IndexOf (sortedNums [i]);
                int temp1 = array1 [i];
                int temp2 = array2 [i];
                array1 [i] = array1 [indexInUnsorted];
                array2 [i] = array2 [indexInUnsorted];
                array1 [indexInUnsorted] = temp1;
                array2 [indexInUnsorted] = temp2;
            }
        }

        private void GetStartInfo () {
            XmlNode node = _xml ["data"]["startInfo"];
            _startInfo.roomID = Convert.ToInt32 (node.Attributes ["roomID"].Value);
            _startInfo.spawnRoomID = Convert.ToInt32 (node.Attributes ["spawnRoomID"].Value);
            _startInfo.money = Convert.ToInt32 (node.Attributes ["money"].Value);
            _startInfo.unitIDs = new List <int> {};
            _startInfo.unitLevels = new List <int> {};
            string [] units = node.Attributes ["units"].Value.Split (new char [] {','});

            if (units [0].Length > 0) {
                foreach (string unit in units) {
                    _startInfo.unitIDs.Add (Convert.ToInt32 (unit.Substring (0, unit.IndexOf ("-"))));
                    _startInfo.unitLevels.Add (Convert.ToInt32 (unit.Substring (unit.IndexOf ("-") + 1)));
                }
            }

            _startInfo.itemIDs = new List <int> {};
            _startInfo.itemAmounts = new List <int> {};

            string [] items = node.Attributes ["items"].Value.Split (new char [] { ',' });

            if (items [0].Length > 0) {
                foreach (string item in items) {
                    _startInfo.itemIDs.Add (Convert.ToInt32 (item.Substring (0, item.IndexOf ("-"))));
                    _startInfo.itemAmounts.Add (Convert.ToInt32 (item.Substring (item.IndexOf ("-") + 1)));
                }
            }
        }
    }
}
