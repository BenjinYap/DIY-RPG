using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace DIY_RPG {

    public class UnitMaster {
        public delegate void UnitEventHandler (Unit unit);
        public static event UnitEventHandler DecreaseHPEvent;
        public static event UnitEventHandler IncreaseHPEvent;
        public static event UnitEventHandler IncreaseEXPEvent;
        public static event UnitEventHandler LevelUpEvent;

        public enum DeadAlive { Dead, Alive }

        public UnitMaster () {
            
        }

        public static Unit MakeUnit (int id, int level) {
            Unit unit = Data._unitInfo.units.Find (theUnit => theUnit.id == id).Clone ();
            unit.level = level;
            int expBase = Data._unitInfo.expNextBase;
            decimal expMult = Data._unitInfo.expNextMult;
            unit.exp = (int) (expBase * (decimal) (1 - Math.Pow ((double) expMult, level - 1)) / (1 - expMult));
            unit.expNext = (int) (expBase * (decimal) (1 - Math.Pow ((double) expMult, level)) / (1 - expMult));
            
            for (int i = 0; i < unit.nativeMoves.Count (); i++) {
                if (level >= unit.nativeMoveLevels [i]) {
                    unit.moves.Add (unit.nativeMoves [i]);
                }
            }
            
            string [] statKeys = new string [] {"HP", "Attack", "SAttack", "Defense", "SDefense", "Speed"};

            for (int i = 0; i < statKeys.Count (); i++) {
                unit.stats [statKeys [i]] += unit.statGrowths [statKeys [i]] * (level - 1);
                unit.oldStats [statKeys [i]] = unit.stats [statKeys [i]];
            }

            unit.hpLeft = unit.stats ["HP"];
            unit.invincible = false;
            return unit;
        }

        public static void DecreaseHP (Unit unit, int damage, bool instant) {
            decimal damageLeft = damage;
            decimal hp = unit.stats ["HP"];
            decimal increment = hp / 100;
            int intervals = (int) Math.Ceiling (damageLeft / increment);

            for (int i = 0; i < intervals; i++) {
                if (damageLeft < increment) {
                    increment = damageLeft;
                }

                if (unit.hpLeft - increment <= 0) {
                    unit.hpLeft = 0;
                } else {
                    unit.hpLeft -= increment;
                }

                damageLeft -= increment;

                DecreaseHPEvent (unit);

                if (unit.hpLeft <= 0) {
                    break;
                }

                if (!instant) {
                    Thread.Sleep (3);
                }
            }
        }

        public static void IncreaseHP (Unit unit, int amount, bool instant) {
            decimal healLeft = amount;
            decimal hp = unit.stats ["HP"];
            decimal increment = hp / 100;
            int intervals = (int) Math.Ceiling (healLeft / increment);

            for (int i = 0; i < intervals; i++) {
                if (healLeft < increment) {
                    increment = healLeft;
                }

                if (unit.hpLeft + increment >= unit.stats ["HP"]) {
                    unit.hpLeft = unit.stats ["HP"];
                } else {
                    unit.hpLeft += increment;
                }

                healLeft -= increment;

                IncreaseHPEvent (unit);

                if (unit.hpLeft >= unit.stats ["HP"]) {
                    break;
                }

                if (!instant) {
                    Thread.Sleep (300);
                }
            }

            unit.hpLeft = (int) unit.hpLeft;
        }

        public static void IncreaseEXP (Unit unit, int exp, bool instant) {
            decimal expLeft = exp;
            int expBase = Data._unitInfo.expNextBase;
            decimal expMult = Data._unitInfo.expNextMult;
            decimal expPrev = (int) (expBase * (decimal) (1 - Math.Pow ((double) expMult, unit.level - 1)) / (1 - expMult));
            decimal increment = (unit.expNext - expPrev) / 100;
            int intervals = (int) Math.Ceiling (expLeft / increment);
            
            for (int i = 0; i < intervals; i++) {
                if (expLeft < increment) {
                    increment = expLeft;
                }
                
                if (unit.exp + increment >= unit.expNext) {    
                    expLeft -= unit.expNext - unit.exp;

                    if (unit.level < Data._unitInfo.maxLevel) {
                        LevelUpUnit (unit);
                        UnitMaster.IncreaseEXP (unit, (int) expLeft, instant);
                    } else {
                        unit.exp = unit.expNext - 1;
                    }

                    break;
                } else {
                    unit.exp += increment;
                    expLeft -= increment;
                }
                
                IncreaseEXPEvent (unit);

                if (expLeft <= 0) {
                    break;
                }

                if (!instant) {
                    Thread.Sleep (3);
                }
            }
        }

        public static void LevelUpUnit (Unit unit) {
            if (unit.level < Data._unitInfo.maxLevel) {
                unit.level++;
                int expBase = Data._unitInfo.expNextBase;
                decimal expMult = Data._unitInfo.expNextMult;
                unit.exp = unit.expNext;
                unit.expNext = (int) (expBase * (decimal) (1 - Math.Pow ((double) expMult, unit.level)) / (1 - expMult));

                for (int i = 0; i < unit.stats.Count (); i++) {
                    double mod = unit.stats [unit.stats.Keys.ElementAt (i)] / (double) unit.oldStats [unit.stats.Keys.ElementAt (i)];
                    unit.oldStats [unit.stats.Keys.ElementAt (i)] += unit.statGrowths [unit.stats.Keys.ElementAt (i)];
                    
                    if (mod != 1) {     //Unit used a stat-changing move in the battle
                        unit.stats [unit.stats.Keys.ElementAt (i)] = (int) (unit.oldStats [unit.stats.Keys.ElementAt (i)] * mod);
                    } else {            //Unit did not use a stat-changing move in the battle
                        unit.stats [unit.stats.Keys.ElementAt (i)] = unit.oldStats [unit.stats.Keys.ElementAt (i)];
                    }
                }

                unit.hpLeft = unit.stats ["HP"];
                LevelUpEvent (unit);
            }
        }

        public static void BlackOutRevive () {
            for (int i = 0; i < GameFile._units.Count (); i++) {
                Unit unit = GameFile._units [i];
                unit.hpLeft = unit.stats ["HP"] / 4;
                GameFile._units [i] = unit;
            }
        }

        public static bool AllDead () {
            if (!GameFile._units.Exists (unit => unit.hpLeft > 0)) {
                return true;
            } else {
                return false;
            }
        }

        public static bool LearnMove (Screen screen, Unit unit, Move move) {
            if (unit.moves.Exists (m => m.id == move.id)) {
                screen.TypeWrite (BuildText (Data._moveInfo.alreadyKnowsMoveText, unit, move), ConsoleColor.DarkYellow, true);
                return false;
            } else {
                if (unit.moves.Count () < Data._unitInfo.maxMoves) {
                    unit.moves.Add (move);
                    screen.TypeWrite (BuildText (Data._moveInfo.learnedMoveText, unit, move), ConsoleColor.DarkYellow, true);
                    return true;
                } else {
                    DeleteMoveScreen dms = new DeleteMoveScreen (unit, move);
                    return dms._learned;
                }
            }
        }

        private static string BuildText (string s, Unit unit, Move move) {
            s = s.Replace ("@", unit.name);
            s = s.Replace ("#", move.name);
            return s;
        }
    }
}