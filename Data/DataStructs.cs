using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace DIY_RPG {
    public enum ClearType {
        AboveDialog, Dialog, Both
    }

    public struct GeneralInfo {
        public string title;
        public int width;
        public int aboveHeight;
        public int dialogHeight;
        public int dividerLine;
        public ConsoleColor selectionColor;
        public ConsoleColor currentSelectionColor;
        public ConsoleColor disabledSelectionColor;
        public List <string> creditLines;
    }

    public struct MoveInfo {
        public string learnedMoveText;
        public string tooManyMovesText;
        public string forgotMoveText;
        public string didNotLearnMoveText;
        public string alreadyKnowsMoveText;
        public List <Move> moves;
    }
    
    public struct UnitInfo {
        public string hpShort;
        public string levelShort;
        public string expShort;
        public Dictionary <string, string> statNames;
        public Dictionary <string, string> statDescs;
        public int nameLength;
        public int min;
        public int max;
        public int maxLevel;
        public string cannotLevelUpText;
        public string tooFewUnitsText;
        public string caughtText;
        public string noSpaceText;
        public string renameQuestion;
        public string newNameBeforeText;
        public string newNameAfterText;
        public string noUnitsText;
        public string gainEXPText;
        public string levelUpText;
        public int maxMoves;
        public int expNextBase;
        public decimal expNextMult;
        public decimal enemyExpMult;
        public List <Unit> units;
    }
    
    public struct BattleInfo {
        public string noUnitsText;
        public string mainMenuText;
        public string fightMenuText;
        public string fight;
        public string bag;
        public string units;
        public string run;
        public int runChanceLevelDiff;
        public string escapeSuccessText;
        public string escapeFailText;
        public string cannotEscapeText;
        public string sendOutUnitText;
        public string swapUnitText;
        public string cannotSwapUnitText;
        public string unitReturnText;
        public string replaceUnitText;
        public string yourUnitDiedText;
        public string allUnitsDeadText;
        public string cannotCatchUnitText;
    }

    public struct ItemInfo {
        public string money;
        public List <string> categories;
        public int maxStack;
        public string noItemsText;
        public List <Item> items;
    }

    public struct StartInfo {
        public int roomID;
        public int spawnRoomID;
        public int money;
        public List <int> unitIDs;
        public List <int> unitLevels;
        public List <int> itemIDs;
        public List <int> itemAmounts;
    }

    public struct Room {
        public int id;
        public string name;
        public List <Option> options;
        public string dialog;
    }

    public struct Option {
        public string text;
        public List <Action> actions;
        public List <OptionItem> ifActions;
    }

    public interface OptionItem { }

    public struct Loop:OptionItem {
        public int repeat;
        public List <OptionItem> items;
    }

    public struct OptionCondition:OptionItem {
        public enum Comparison { MoneyGreaterThan, MoneyLessThan, AllUnitsDead, LastBattleWon, HasItem, HasUnit, HasLevelRange }
        
        public Comparison comparison;
        public string expression;
        public List <OptionItem> trueItems;
        public List <OptionItem> falseItems;
    }

    public struct Action:OptionItem {
        public enum Type { GetUnit, GetItem, GetMoney, LoseItem, LoseMoney, UnitGainEXP, AllUnitsGainEXP, UnitLevelUp, AllUnitsLevelUp, FightWild, FightTrainer, SetUnitHP, SetAllUnitsHP, ChargeUnitMoves, ChargeAllUnitsMoves, TeachUnitMove, TeachAllUnitMove, Text, Move, ItemShop, SetAsSpawn, Options, Break, SwapRoomIDs }
        
        public List<Option> options;   //The list of options an action might have
        public Type type;             //The type of action
        public Dictionary <string, string> vars;
    }

    public class Unit {
        public string name;
        public int id;
        public int level;
        public string desc;
        public decimal exp;
        public long expNext;
        public List <Move> moves;
        public List <Move> nativeMoves;
        public List <int> nativeMoveLevels;
        public Dictionary <string, int> stats;          //Current stats
        public Dictionary <string, int> oldStats;       //Stats before stat-increasing move
        public Dictionary <string, int> statGrowths;    //Stats increase per level
        public decimal hpLeft;        
        public List <Move.IfPart> activeMoveIfParts;
        public Ailment ailment;
        public bool invincible;
        public int enemyBaseExp;
        public string encounterText;
        public string diedText;
        public string playerLoseText;
        public string playerWinText;
        public string moneyGainText;
        public string moneyLoseText;
        public int moneyGain;
        public int moneyLose;

        public Unit Clone () {
            Unit clone = (Unit) this.MemberwiseClone ();
            clone.moves = new List <Move> {};
            clone.stats = new Dictionary <string, int> {};

            foreach (KeyValuePair<string, int> pair in this.stats) {
                clone.stats [pair.Key] = pair.Value;
            }

            clone.oldStats = new Dictionary <string, int> {};
            clone.activeMoveIfParts = new List <Move.IfPart> {};
            clone.ailment = new Ailment ();
            return clone;
        }
    }

    public struct Ailment {
        public enum Type { TurnDamage, DoNothing, HurtSelf }
        public enum DamageType { Normal, Special }

        public int id;
        public string name;
        public Type type;
        public DamageType damageType;
        public string successText;
        public string missText;
        public string noEffectText;
        public string tag;
        public int turns;
        public double chance;
        public double mod;
        public int originalAttack;
    }

    public struct Move {
        public interface IfPart {
            IfPart Clone ();
        };

        public int id;
        public string name;
        public string desc;
        public int uses;
        public int usesLeft;
        public List <Move.IfPart> ifParts;

        public struct MoveCondition:IfPart {
            public enum Comparison { LastPartHit, LastPartMiss, HasAilmentSelf, HasAilmentEnemy, HPSelfAmount, HPSelfPerc, HPEnemyAmount, HPEnemyPerc  }

            public Comparison comparison;
            public string expression;
            public List<IfPart> trueIfParts;
            public List<IfPart> falseIfParts;

            public IfPart Clone () {
                MoveCondition condition = (MoveCondition) this.MemberwiseClone ();
                return condition;
            }
        }

        public class Part:IfPart {
            public enum Target { Self, Enemy }

            public Move move;
            public bool use;
            public Target target;
            public string effect;
            public double effectNum;
            public double accuracy;
            public string useText;
            public string successText;
            public string missText;
            public string noEffectText;
            public bool effectDone;

            public IfPart Clone () {
                Part clone = (Part) this.MemberwiseClone ();
                return clone;
            }
        }
    }

    public struct Trainer {
        public int id;
        public string name;
        public List <Unit> units;
        public string encounterText;
        public string sendOutUnitText;
        public string unitDiedText;
        public string allUnitsDiedText;
        public string playerLoseText;
        public string playerWinText;
        public string moneyGainText;
        public string moneyLoseText;
        public int moneyGain;
        public int moneyLose;

        public Trainer (string n) {
            id = 0;
            name = n;
            units = new List <Unit> {};
            encounterText = "";
            sendOutUnitText = "";
            unitDiedText = "";
            allUnitsDiedText = "";
            playerLoseText = "";
            playerWinText = "";
            moneyGainText = "";
            moneyLoseText = "";
            moneyGain = 0;
            moneyLose = 0;
        }

        public Trainer Clone () {
            Trainer clone = (Trainer) this.MemberwiseClone ();
            clone.units = new List<Unit> { };

            for (int i = 0; i < this.units.Count (); i++) {
                clone.units.Add (UnitMaster.MakeUnit (this.units [i].id, this.units [i].level));
            }

            return clone;
        }
    }

    public struct Item {
        public enum Effect { HealHP, Revive, Cure, TeachMove, Catch, CatchHPBased, GiveEXP, GiveLevel }

        public int id;
        public string name;
        public string desc;
        public string category;
        public string flags;
        public Effect effect;
        public double effectNum;
        public double accuracy;
        public string useText;
        public string successText;
        public string failText;
        public bool unlimited;
    }

    public class ItemStack {
        public int itemId;
        public string category;
        public int amount;
    }

    public struct ItemShop {
        public enum SortMode { ByName, ByPrice, ByInsertOrder }

        public int id;
        public string name;
        public string greeting;
        public string sellTabText;
        public string buyTabText;
        public string boughtText;
        public string cannotAffordText;
        public string alreadyHaveItemText;
        public List <int> sellItemIDs;
        public List <int> sellPrices;
        public string soldText;
        public string notEnoughText;
        public List <int> buyItemIDs;
        public List <int> buyPrices;
        public SortMode sortMode;
    }

    public struct UnitShop {
        public enum SortMode { ByName, ByLevel, ByPrice, ByInsertOrder }

        public int id;
        public string name;
        public string greeting;
        public string sellTabText;
        public string buyTabText;
        public string boughtText;
        public string cannotAffordText;
        public List <Unit> sellUnits;
        public List<int> sellPrices;
        public string buyUnitsDesc;
        public string soldText;
        public string noUnitsText;
        public List<int> buyUnitIDs;
        public List<int> buyPrices;
        public SortMode sortMode;
        public bool canRenameUnits;
    }
}
