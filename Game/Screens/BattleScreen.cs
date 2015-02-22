using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace DIY_RPG {

    public class BattleScreen:Screen {
        private enum BoxToDraw { Player, Enemy }
        private enum SelectionState { Main, Fight, Bag, Unit, Run }
        private enum FirstUnit { Player, Enemy }

        private bool _fightOver = false;
        private bool _caughtUnit = false;
        private Unit _lastCaughtUnit = new Unit ();
        private int _selectIndex;
        private Unit _player;
        private Unit _enemy;
        private Trainer _trainer = new Trainer ("");
        private List <string> _playerBox = new List <string> {};
        private List <string> _enemyBox = new List <string> {};
        private bool _canEscape;
        private bool _canCatch;
        private bool _lastPartHit = false;

        private SelectionState _selState;

        public BattleScreen (Unit enemy, bool canEscape, bool canCatch):base () {
            _canEscape = canEscape;
            _canCatch = canCatch;
            DrawScreenTitle ("Battle");
            Clear (ClearType.Both);
            _player = GameFile._units.Find (p => p.hpLeft > 0);
            _enemy = enemy;
            SendOutUnit (false, false, _enemy);
            SendOutUnit (true, false, _player);
            Start ();
        }

        public BattleScreen (Trainer trainer, bool canEscape, bool canCatch):base () {
            _canEscape = canEscape;
            _canCatch = canCatch;
            DrawScreenTitle ("Battle");
            Clear (ClearType.Both);
            _player = GameFile._units.Find (p => p.hpLeft > 0);
            _trainer = trainer;
            TypeWrite (_trainer.encounterText, ConsoleColor.DarkYellow, true);
            SendOutUnit (false, true, _trainer.units [0]);
            SendOutUnit (true, false, GameFile._units.Find (unit => unit.hpLeft > 0));
            Start ();
        }

        private void SendOutUnit (bool isPlayer, bool isTrainer, Unit unit) {
            if (isPlayer) {
                _player = unit;
                TypeWrite (BuildText (Data._battleInfo.sendOutUnitText, _player), ConsoleColor.DarkYellow, true);
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
            } else {
                _enemy = unit;

                if (isTrainer) {
                    TypeWrite (BuildText (_trainer.sendOutUnitText, _enemy), ConsoleColor.DarkYellow, true);
                } else {
                    TypeWrite (_enemy.encounterText, ConsoleColor.DarkYellow, true);
                }

                DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
            }
        }

        private void Start () {
            GameFile._inBattle = true;
            _selState = SelectionState.Main;

            while (true) {
                Clear (ClearType.Both);
                DrawScreenTitle ("Battle");
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
                DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);

                if (_selState == SelectionState.Main) {
                    Write (BuildText (Data._battleInfo.mainMenuText, _player), 2 + Data._generalInfo.aboveHeight + 1, ConsoleColor.DarkYellow);
                    GetDialogSelection (new List<string> { Data._battleInfo.fight, Data._battleInfo.bag, Data._battleInfo.units, Data._battleInfo.run });
                    
                    if (_selectIndex == 0) {
                        _selState = SelectionState.Fight;
                    } else if (_selectIndex == 1) {
                        _selState = SelectionState.Bag;
                    } else if (_selectIndex == 2) {
                        _selState = SelectionState.Unit;
                    } else if (_selectIndex == 3) {
                        _selState = SelectionState.Run;
                    }

                    _selectIndex = 0;
                } else if (_selState == SelectionState.Fight) {
                    if (_player.activeMoveIfParts.Count () == 0) {
                        Write (BuildText (Data._battleInfo.fightMenuText, _player), 2 + Data._generalInfo.aboveHeight + 1, ConsoleColor.DarkYellow);
                        List <string> moves = Array.ConvertAll <Move, string> (_player.moves.ToArray (), delegate (Move move) { return move.name; } ).ToList ();

                        while (true) {
                            GetDialogSelection (moves);
                            
                            if (_selectIndex == -1) {
                                _selectIndex = 0;
                                _selState = SelectionState.Main;
                                break;
                            }

                            if (_player.moves [_selectIndex].usesLeft > 0 || _player.moves [_selectIndex].uses == -1) {
                                Fight (_player.moves [_selectIndex]);
                                _selState = SelectionState.Main;
                                break;
                            }
                        }
                    } else {
                        Fight (new Move ());
                        _selState = SelectionState.Main;
                    }

                    _selectIndex = 0;
                } else if (_selState == SelectionState.Bag) {
                    if (GameFile._items.Count () > 0) {
                        MenuBagScreen mbs = new MenuBagScreen ();
                        mbs.UseItemEvent -= onUseItem;
                        mbs.UseItemEvent += onUseItem;
                        mbs.Start ();
                        
                        if (mbs._usedItem) {
                            _selectIndex = 0;

                            if (_caughtUnit) {                  //If the wild unit has been caught
                                if (_trainer.name.Length == 0) {    //If it's a wild battle
                                    GameFile._lastBattleWon = true;
                                    break;                              //End the battle
                                } else {                            //If it's a trainer battle
                                    if (_trainer.units.FindIndex (p => p.hpLeft > 0) == -1) { //If the trainer has no more units
                                        GameFile._lastBattleWon = true;
                                        break;                              //End the battle
                                    } else {                            //If the trainer has more units
                                        SendOutUnit (false, true, _trainer.units.Find (p => p.hpLeft > 0));
                                    }
                                }
                            } else {                            //If not
                                Fight (new Move ());                //Start the battle turn with player doing nothing
                            }
                        } else {
                            _selectIndex = 1;
                        }
                    } else {
                        TypeWrite (Data._itemInfo.noItemsText, ConsoleColor.DarkYellow, true);
                    }

                    _selState = SelectionState.Main;
                } else if (_selState == SelectionState.Unit) {
                    MenuUnitsScreen mps = new MenuUnitsScreen (0, BuildText (Data._battleInfo.swapUnitText, _player), BuildText (Data._battleInfo.cannotSwapUnitText, _player));

                    for (int i = 0; i < GameFile._units.Count (); i++) {
                        mps._selectionAbles [i] = (GameFile._units [i].hpLeft <= 0 || GameFile._units [i].Equals (_player)) ? false : true;
                    }

                    int selection = mps.GetSelection (true, new List<string> { "Select", "Stats", "Cancel"});

                    if (selection != -1) {
                        _player.activeMoveIfParts = new List <Move.IfPart> {};
                        DrawScreenTitle ("Battle");
                        Clear (ClearType.Both);
                        DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
                        DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
                        TypeWrite (BuildText (Data._battleInfo.unitReturnText, _player), ConsoleColor.DarkYellow, true);
                        Clear (ClearType.Both);
                        DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
                        SendOutUnit (true, false, GameFile._units [selection]);
                        Fight (new Move ());
                        _selectIndex = 0;
                    } else {
                        _selectIndex = 2;
                    }

                    _selState = SelectionState.Main;
                } else if (_selState == SelectionState.Run) {
                    if ((_trainer.name.Length > 0 && _canEscape) || _trainer.name.Length == 0) {
                        int averageLevelPlayer = 0;
                        int averageLevelEnemy = 0;

                        if (_trainer.name.Length > 0) {
                            for (int i = 0; i < _trainer.units.Count (); i++) {
                                averageLevelEnemy += _trainer.units [i].level;
                            }

                            averageLevelEnemy /= _trainer.units.Count ();
                        } else {
                            averageLevelEnemy = _enemy.level;
                        }

                        for (int i = 0; i < GameFile._units.Count (); i++) {
                            averageLevelPlayer += GameFile._units [i].level;
                        }

                        averageLevelPlayer /= GameFile._units.Count ();
                        double chance = 0.5 + 0.5 * ((averageLevelPlayer - averageLevelEnemy) / (double) Data._battleInfo.runChanceLevelDiff);
                        
                        if (chance >= PreGameScreen._rand.NextDouble ()) {
                            GameFile._lastBattleWon = false;
                            TypeWrite (Data._battleInfo.escapeSuccessText, ConsoleColor.DarkYellow, true);
                            break;
                        } else {
                            TypeWrite (Data._battleInfo.escapeFailText, ConsoleColor.DarkYellow, true);
                            Fight (new Move ());
                        }
                    } else {
                        TypeWrite (Data._battleInfo.cannotEscapeText, ConsoleColor.DarkYellow, true);
                    }

                    _selState = SelectionState.Main;
                }

                if (_fightOver) {
                    break;
                }
            }
            
            if (!UnitMaster.AllDead () && GameFile._lastBattleWon) {
                if ((_trainer.name.Length == 0 && !_lastCaughtUnit.Equals (_enemy)) || (_trainer.name.Length > 0 && !_lastCaughtUnit.Equals (_enemy))) {
                    if (_trainer.name.Length > 0) {
                        if (_trainer.moneyGain > 0) {
                            TypeWrite (_trainer.playerWinText.Replace ("@", _trainer.name), ConsoleColor.DarkYellow, true);
                            GameFile._money += _trainer.moneyGain;
                            string winMoneyText = _trainer.moneyGainText;
                            winMoneyText = winMoneyText.Replace ("@", _trainer.name);
                            winMoneyText = winMoneyText.Replace ("#", _trainer.moneyGain.ToString ());
                            TypeWrite (winMoneyText, ConsoleColor.DarkYellow, true);
                        }
                    } else {
                        if (_enemy.moneyGain > 0) {
                            TypeWrite (BuildText (_enemy.playerWinText, _enemy), ConsoleColor.DarkYellow, true);
                            GameFile._money += _enemy.moneyGain;
                            string winMoneyText = _enemy.moneyGainText;
                            winMoneyText = winMoneyText.Replace ("@", _enemy.name);
                            winMoneyText = winMoneyText.Replace ("#", _enemy.moneyGain.ToString ());
                            TypeWrite (winMoneyText, ConsoleColor.DarkYellow, true);
                        }
                    }
                }
            } else if (UnitMaster.AllDead ()) {
                TypeWrite (Data._battleInfo.allUnitsDeadText, ConsoleColor.DarkYellow, true);
                GameFile._lastBattleWon = false;

                if (_enemy.hpLeft > 0) {
                    if (_trainer.name.Length > 0) {
                        if (_trainer.moneyLose > 0) {
                            TypeWrite (_trainer.playerLoseText, ConsoleColor.DarkYellow, true);
                            GameFile._money = (GameFile._money < _trainer.moneyLose) ? 0 : GameFile._money - _trainer.moneyLose;
                            string moneyLoseText = _trainer.moneyLoseText;
                            moneyLoseText = moneyLoseText.Replace ("#", _trainer.moneyLose.ToString ());
                            TypeWrite (moneyLoseText, ConsoleColor.DarkYellow, true);
                        }
                    } else {
                        if (_enemy.moneyLose > 0) {
                            TypeWrite (_enemy.playerLoseText, ConsoleColor.DarkYellow, true);
                            GameFile._money = (GameFile._money < _enemy.moneyLose) ? 0 : GameFile._money - _enemy.moneyLose;
                            string moneyLoseText = _enemy.moneyLoseText;
                            moneyLoseText = moneyLoseText.Replace ("#", _enemy.moneyLose.ToString ());
                            TypeWrite (moneyLoseText, ConsoleColor.DarkYellow, true);
                        }
                    }
                }
            }

            foreach (Unit p in GameFile._units) {
                for (int i = 0; i < p.stats.Count (); i++) {
                    p.stats [p.stats.Keys.ElementAt (i)] = p.oldStats [p.stats.Keys.ElementAt (i)];
                }
            }

            Clear (ClearType.Both);
            GameFile._inBattle = false;
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
            } else if (item.effect == Item.Effect.Catch || item.effect == Item.Effect.CatchHPBased) {
                Clear (ClearType.Both);
                DrawScreenTitle ("Battle");
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
                DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);

                if ((_trainer.name.Length > 0 && _canCatch) || _trainer.name.Length == 0) {
                    double chance = item.effectNum;

                    if (item.effect == Item.Effect.CatchHPBased) {
                        chance *= 1 - (double) (_enemy.hpLeft / _enemy.stats ["HP"]) * 0.5;
                    }

                    if (chance >= PreGameScreen._rand.NextDouble ()) {
                        _caughtUnit = true;
                        _lastCaughtUnit = _enemy;
                        _trainer.units.Remove (_enemy);
                        Clear (ClearType.Both);
                        DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);

                        if (GameFile._units.Count () < Data._unitInfo.max) {
                            new GetUnitScreen (_enemy, Data._unitInfo.caughtText, true);
                        } else {
                            TypeWrite (Data._unitInfo.noSpaceText, ConsoleColor.DarkYellow, true);
                        }
                    }
                } else {
                    TypeWrite (BuildText (Data._battleInfo.cannotCatchUnitText, _enemy), ConsoleColor.DarkYellow, true);
                }
            } else if (item.effect == Item.Effect.GiveEXP) {
                IncreaseEXP (GameFile._units [unitIndex], (int) item.effectNum);
            } else if (item.effect == Item.Effect.GiveLevel) {
                UnitMaster.LevelUpEvent -= onLevelUp;
                UnitMaster.LevelUpEvent += onLevelUp;
                UnitMaster.LevelUpUnit (GameFile._units [unitIndex]);
            }
        }

        private void DrawDialogSelection (string s, int line, int left, ConsoleColor color) {
            Console.SetCursorPosition (left, line);
            Console.ForegroundColor = color;
            Console.Write (s);
        }

        private void GetDialogSelection (List <string> rawSelections) {
            ConsoleKey key = ConsoleKey.EraseEndOfFile;
            List <string> selections = new List <string> {};
            List <int> lines = new List <int> {};

            for (int i = 0; i < rawSelections.Count (); i++) {
                string s = new string (' ', (Data._generalInfo.width / 2 - 1 - rawSelections [i].Length) / 2) + rawSelections [i];
                selections.Add (s);
                lines.Add (i / 2 + Data._generalInfo.dividerLine + 2);
            }

            while (key != ConsoleKey.Enter) {
                for (int i = Data._generalInfo.dividerLine + 2; i < Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight + 1; i++) {
                    Console.SetCursorPosition (0, i);
                    Console.Write (new string (' ', Data._generalInfo.width));
                }

                for (int i = 0; i < selections.Count (); i++) {
                    ConsoleColor color = (_selectIndex == i) ? Data._generalInfo.currentSelectionColor : Data._generalInfo.selectionColor;
                    int left = 0;

                    if (Data._generalInfo.width % 2 == 0) {
                        left = 2 + Data._generalInfo.width / 2 * (i % 2) + i % 2;
                    } else {
                        left = 2 + Data._generalInfo.width / 2 * (i % 2) + i % 2;
                    }
                    
                    if (lines [i] >= Data._generalInfo.dividerLine + 2 && lines [i] <= Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight) {
                        DrawDialogSelection (selections [i], lines [i], left, color);
                    }

                    if (_selState == SelectionState.Fight) {
                        if (_player.moves [_selectIndex].uses != -1) {
                            Write (_player.moves [_selectIndex].usesLeft.ToString () + "/" + _player.moves [_selectIndex].uses.ToString () + " uses left", Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight + 1, Data._generalInfo.selectionColor);
                        } else {
                            Write ("∞/∞ uses left", Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight + 1, Data._generalInfo.selectionColor);
                        }
                    }
                }

                Console.SetCursorPosition (0, Data._generalInfo.dividerLine + 2);
                Console.ForegroundColor = Data._generalInfo.selectionColor;

                if (lines [0] < Data._generalInfo.dividerLine + 2) {
                    Console.Write ("↑");
                    Console.CursorLeft = Console.BufferWidth - 1;
                    Console.Write ("↑");
                } else {
                    Console.Write (" ");
                    Console.CursorLeft = Console.BufferWidth - 1;
                    Console.Write (" ");
                }

                Console.SetCursorPosition (0, Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight);

                if (lines [lines.Count () - 1] > Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight) {
                    Console.Write ("↓");
                    Console.CursorLeft = Console.BufferWidth - 1;
                    Console.Write ("↓");
                } else {
                    Console.Write (" ");
                    Console.CursorLeft = Console.BufferWidth - 1;
                    Console.Write (" ");
                }

                key = Console.ReadKey (true).Key;

                switch (key) {
                    case ConsoleKey.W:
                        _selectIndex = (_selectIndex > 1) ? _selectIndex - 2 : _selectIndex;

                        if (lines [_selectIndex] < Data._generalInfo.dividerLine + 2) {
                            for (int i = 0; i < lines.Count (); i++) {
                                lines [i]++;
                            }
                        }

                        break;
                    case ConsoleKey.S:
                        _selectIndex = (_selectIndex % 2 == 1 && _selectIndex + 1 == selections.Count () - 1) ? _selectIndex + 1 : _selectIndex;
                        _selectIndex = (_selectIndex + 2 <= selections.Count () - 1) ? _selectIndex + 2 : _selectIndex;

                        if (lines [_selectIndex] > Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight) {
                            for (int i = 0; i < lines.Count (); i++) {
                                lines [i]--;
                            }
                        }

                        break;
                    case ConsoleKey.A:
                        _selectIndex = (_selectIndex % 2 == 0) ? _selectIndex : _selectIndex - 1;
                        break;
                    case ConsoleKey.D:
                        _selectIndex = (_selectIndex % 2 == 0 && _selectIndex + 1 <= selections.Count () - 1) ? _selectIndex + 1 : _selectIndex;
                        break;
                    case ConsoleKey.Escape:
                        _selectIndex = -1;
                        break;
                }

                if (_selectIndex == -1) {
                    break;
                }
            }

            Console.SetCursorPosition (Data._generalInfo.width, Data._generalInfo.dividerLine + 2);
            Console.Write (" ");
            Console.SetCursorPosition (Data._generalInfo.width, Data._generalInfo.dividerLine + Data._generalInfo.dialogHeight);
            Console.Write (" ");
        }

        private void Fight (Move playerMove) {
            DrawScreenTitle ("Battle");
            Clear (ClearType.Both);
            DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
            DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);

            int roll1 = PreGameScreen._rand.Next (0, _player.stats ["Speed"]);
            int roll2 = PreGameScreen._rand.Next (0, _enemy.stats ["Speed"]);
            roll1 = 1000;
            //roll2 = 1000;
            _player.stats ["Attack"] = 1000;
            //_enemy.stats ["Attack"] = 1000;

            Unit firstUnit = (roll1 >= roll2) ? _player : _enemy;
            Unit secondUnit = (roll1 >= roll2) ? _enemy : _player;
            Move firstMove = (roll1 >= roll2) ? DataCloner.CloneMove (playerMove) : DataCloner.CloneMove (_enemy.moves [PreGameScreen._rand.Next (0, _enemy.moves.Count ())]);
            Move secondMove = (roll1 >= roll2) ? DataCloner.CloneMove (_enemy.moves [PreGameScreen._rand.Next (0, _enemy.moves.Count ())]) : DataCloner.CloneMove (playerMove);
            //firstMove = DataCloner.CloneMove (_enemy.moves [0]);//scratch
            //firstMove = DataCloner.CloneMove (_enemy.moves [1]);//Suicide
            //firstMove = DataCloner.CloneMove (_enemy.moves [2]);//nothing
            //firstMove = DataCloner.CloneMove (_enemy.moves [3]);//kamikaze
            //secondMove = DataCloner.CloneMove (_enemy.moves [0]);//scratch
            //secondMove = DataCloner.CloneMove (_enemy.moves [1]);//Suicide
            //secondMove = DataCloner.CloneMove (_enemy.moves [1]);//nothing
            //secondMove = DataCloner.CloneMove (_enemy.moves [3]);//kamikaze
            
            if (firstUnit.hpLeft > 0) {
                HandleAilment (firstUnit, ref firstMove);
            }

            if (!_fightOver && firstUnit.hpLeft > 0) {
                UseMove (firstUnit, secondUnit, firstMove, secondMove);
            }

            if (secondUnit.hpLeft <= 0) {
                FaintUnit (secondUnit.Equals (_player), firstUnit.hpLeft > 0);
            }

            if (firstUnit.hpLeft <= 0) {
                FaintUnit (firstUnit.Equals (_player), secondUnit.hpLeft > 0);
            }

            HandleDead (ref secondUnit, ref secondMove, ref firstUnit, ref firstMove);
            
            if (!_fightOver && secondUnit.hpLeft > 0) {
                HandleAilment (secondUnit, ref secondMove);
            }

            if (!_fightOver && secondUnit.hpLeft > 0) {
                UseMove (secondUnit, firstUnit, secondMove, firstMove);
            }
            
            if (!_fightOver) {
                if (firstUnit.hpLeft <= 0) {
                    FaintUnit (firstUnit.Equals (_player), secondUnit.hpLeft > 0);
                }

                if (secondUnit.hpLeft <= 0) {
                    FaintUnit (secondUnit.Equals (_player), firstUnit.hpLeft > 0);
                }

                HandleDead (ref firstUnit, ref firstMove, ref secondUnit, ref secondMove);
            }
        }

        private void HandleAilment (Unit owner, ref Move ownerMove) {
            if (owner.ailment.turns > 0) {
                Ailment ail = owner.ailment;
                double chance = PreGameScreen._rand.NextDouble ();

                if (ail.chance >= chance) {
                    if (ail.type == Ailment.Type.TurnDamage) {
                        int damage = 0;
                        
                        if (ail.damageType == Ailment.DamageType.Normal) {
                            damage = (int) (ail.mod * ail.originalAttack) - owner.stats ["Defense"];
                        } else {
                            damage = (int) (ail.mod * ail.originalAttack) - owner.stats ["SDefense"];
                        }
                        
                        damage = (damage <= 0) ? 0 : damage;
                        
                        if (damage == 0) {
                            TypeWrite (BuildText (ail.noEffectText, owner), ConsoleColor.DarkYellow, true);
                        } else {
                            TypeWrite (BuildText (ail.successText, owner), ConsoleColor.DarkYellow, true);
                        }

                        DecreaseHP (owner, damage);
                    } else if (ail.type == Ailment.Type.DoNothing) {
                        ownerMove = new Move ();
                        ownerMove.ifParts = new List <Move.IfPart> {};
                        TypeWrite (BuildText (ail.successText, owner), ConsoleColor.DarkYellow, true);
                    } else if (ail.type == Ailment.Type.HurtSelf) {
                        int damage = 0;
                        
                        if (owner.stats ["Attack"] >= owner.stats ["SAttack"]) {
                            damage = (int) (ail.mod * owner.stats ["Attack"]) - owner.stats ["Defense"];
                        } else {
                            damage = (int) (ail.mod * owner.stats ["SAttack"]) - owner.stats ["SDefense"];
                        }

                        if (damage > 0) {
                            TypeWrite (BuildText (ail.successText, owner), ConsoleColor.DarkYellow, true);
                            DecreaseHP (owner, damage);
                        } else {
                            TypeWrite (BuildText (ail.noEffectText, owner), ConsoleColor.DarkYellow, true);
                        }

                        ownerMove = new Move ();
                        ownerMove.ifParts = new List<Move.IfPart> { };
                    }
                } else {
                    TypeWrite (BuildText (ail.missText, owner), ConsoleColor.DarkYellow, true);
                }

                owner.ailment.turns--;
            }
            
            DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
            DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
        }

        private void UseMove (Unit user, Unit other, Move userMove, Move otherMove) {
            if (user.activeMoveIfParts.Count () > 0) {
                ProcessMoveIfPart (user, other);
            } else if (userMove.ifParts.Count () > 0) {
                user.activeMoveIfParts = new List <Move.IfPart> {};

                foreach (Move.IfPart ifPart in userMove.ifParts) {
                    user.activeMoveIfParts.Add (ifPart.Clone ());
                }
                
                ProcessMoveIfPart (user, other);
            }
        }

        private void ProcessMoveIfPart (Unit user, Unit other) {
            if (user.activeMoveIfParts [0].GetType () == typeof (Move.MoveCondition)) {
                Move.MoveCondition condition = (Move.MoveCondition) user.activeMoveIfParts [0];
                user.activeMoveIfParts.RemoveAt (0);
                bool isTrue = false;

                switch (condition.comparison) {
                    case Move.MoveCondition.Comparison.LastPartHit:
                        if (_lastPartHit) {
                            isTrue = true;
                        }

                        break;
                    case Move.MoveCondition.Comparison.LastPartMiss:
                        if (!_lastPartHit) {
                            isTrue = true;
                        }

                        break;
                    case Move.MoveCondition.Comparison.HasAilmentSelf:
                        if (user.ailment.id == Convert.ToInt32 (condition.expression) && user.ailment.turns > 0) {
                            isTrue = true;
                        }

                        break;
                    case Move.MoveCondition.Comparison.HasAilmentEnemy:
                        if (other.ailment.id == Convert.ToInt32 (condition.expression) && other.ailment.turns > 0) {
                            isTrue = true;
                        }

                        break;
                    case Move.MoveCondition.Comparison.HPSelfAmount:
                        int hpMin = Convert.ToInt32 (condition.expression.Substring (0, condition.expression.IndexOf ("-")));
                        int hpMax = Convert.ToInt32 (condition.expression.Substring (condition.expression.IndexOf ("-") + 1));

                        if (user.hpLeft >= hpMin && user.hpLeft <= hpMax) {
                            isTrue = true;
                        }

                        break;
                    case Move.MoveCondition.Comparison.HPSelfPerc:
                        hpMin = (int) (user.stats ["HP"] * Convert.ToDouble (condition.expression.Substring (0, condition.expression.IndexOf ("-"))));
                        hpMax = (int) (user.stats ["HP"] * Convert.ToDouble (condition.expression.Substring (condition.expression.IndexOf ("-") + 1)));

                        if (user.hpLeft >= hpMin && user.hpLeft <= hpMax) {
                            isTrue = true;
                        }

                        break;
                    case Move.MoveCondition.Comparison.HPEnemyAmount:
                        hpMin = Convert.ToInt32 (condition.expression.Substring (0, condition.expression.IndexOf ("-")));
                        hpMax = Convert.ToInt32 (condition.expression.Substring (condition.expression.IndexOf ("-") + 1));

                        if (other.hpLeft >= hpMin && other.hpLeft <= hpMax) {
                            isTrue = true;
                        }

                        break;
                    case Move.MoveCondition.Comparison.HPEnemyPerc:
                        hpMin = (int) (other.stats ["HP"] * Convert.ToDouble (condition.expression.Substring (0, condition.expression.IndexOf ("-"))));
                        hpMax = (int) (other.stats ["HP"] * Convert.ToDouble (condition.expression.Substring (condition.expression.IndexOf ("-") + 1)));
                        
                        if (other.hpLeft >= hpMin && other.hpLeft <= hpMax) {
                            isTrue = true;
                        }

                        break;
                }

                if (isTrue) {
                    user.activeMoveIfParts.InsertRange (0, condition.trueIfParts);
                } else {
                    user.activeMoveIfParts.InsertRange (0, condition.falseIfParts);
                }

                if (user.hpLeft > 0 && user.activeMoveIfParts.Count () > 0) {
                    if (user.activeMoveIfParts [0].GetType () == typeof (Move.Part)) {
                        Move.Part nextPart = (Move.Part) user.activeMoveIfParts [0];

                        if (!nextPart.effect.Contains ("Wait")) {
                            ProcessMoveIfPart (user, other);
                        }
                    } else if (user.activeMoveIfParts [0].GetType () == typeof (Move.MoveCondition)) {
                        ProcessMoveIfPart (user, other);
                    }
                }
            } else {
                Move.Part part = (Move.Part) user.activeMoveIfParts [0];
                Unit target = (part.target == Move.Part.Target.Enemy) ? other : user;

                if (part.useText.Length > 0) {
                    TypeWrite (GetMovePartString (part.useText, user), ConsoleColor.DarkYellow, true);
                }
                
                double acc = PreGameScreen._rand.NextDouble ();

                if (part.accuracy >= acc) {
                    bool success = true;
                    _lastPartHit = true;

                    if (part.effect.Contains ("Wait")) {
                        if (part.effectNum > 0) {
                            success = true;
                            part.effectNum--;
                        }
                    } else if (part.effect.Contains ("Damage")) {
                        int damage = 0;

                        if (part.effect.Contains ("Normal")) {
                            damage = (int) (part.effectNum * user.stats ["Attack"]) - target.stats ["Defense"];
                        } else if (part.effect.Contains ("Special")) {
                            damage = (int) (part.effectNum * user.stats ["SAttack"]) - target.stats ["SDefense"];
                        }

                        damage = (damage <= 0) ? 0 : damage;

                        if (damage == 0) {
                            success = false;
                        } else {
                            success = true;
                            DecreaseHP (target, damage);
                        }
                    } else if (part.effect.Contains ("Heal")) {
                        int amount = 0;

                        if (part.effect.Contains ("Normal")) {
                            amount = (int) (part.effectNum * user.stats ["Attack"]);
                        } else if (part.effect.Contains ("Special")) {
                            amount = (int) (part.effectNum * user.stats ["SAttack"]);
                        }

                        if (target.hpLeft >= target.stats ["HP"]) {
                            success = false;
                        } else {
                            success = true;
                            IncreaseHP (target, amount);
                        }
                    } else if (part.effect.Contains ("Ailment")) {
                        success = true;
                        Ailment ail = Data._ailments.Find (a => a.id == Convert.ToInt32 (part.effect.Substring (7)));
                        ail.mod = part.effectNum;
                        ail.originalAttack = (ail.damageType == Ailment.DamageType.Normal) ? user.stats ["Attack"] : user.stats ["SAttack"];
                        target.ailment = ail;
                    } else if (part.effect.Contains ("Increase")) {
                        string statKey = part.effect.Substring (8);

                        if (target.oldStats [statKey] * part.effectNum > target.stats [statKey]) {
                            success = true;
                            target.stats [statKey] = (int) (target.stats [statKey] * part.effectNum);

                            if (target.stats [statKey] > target.oldStats [statKey] * part.effectNum) {
                                target.stats [statKey] = (int) (user.oldStats [statKey] * part.effectNum);
                            }

                            if (statKey == "HP") {
                                target.hpLeft *= (decimal) part.effectNum;
                                target.hpLeft = (target.hpLeft > target.stats ["HP"]) ? target.stats ["HP"] : target.hpLeft;
                            }

                        } else {
                            success = false;
                        }
                    } else if (part.effect.Contains ("Decrease")) {
                        string statKey = part.effect.Substring (8);

                        if (target.oldStats [statKey] * part.effectNum < target.stats [statKey]) {
                            success = true;
                            target.stats [statKey] = (int) (target.stats [statKey] * part.effectNum);
                            
                            if (target.stats [statKey] < target.oldStats [statKey] * part.effectNum) {
                                target.stats [statKey] = (int) (target.oldStats [statKey] * part.effectNum);
                            }

                            if (statKey == "HP") {
                                target.hpLeft *= (decimal) part.effectNum;
                                target.hpLeft = (target.hpLeft > target.stats ["HP"]) ? target.stats ["HP"] : target.hpLeft;
                            }

                        } else {
                            success = false;
                        }
                    }

                    if (success && part.successText.Length > 0) {
                        TypeWrite (BuildText (part.successText, user, target), ConsoleColor.DarkYellow, true);
                    } else if (!success && part.noEffectText.Length > 0) {
                        TypeWrite (BuildText (part.noEffectText, user, target), ConsoleColor.DarkYellow, true);
                    }
                } else {
                    _lastPartHit = false;

                    if (part.missText.Length > 0) {
                        TypeWrite (BuildText (part.missText, user, target), ConsoleColor.DarkYellow, true);
                    }
                }

                if (part.use && part.move.uses != -1) {
                    part.move.usesLeft--;
                    user.moves [user.moves.FindIndex (m => m.id == part.move.id)] = part.move;
                }

                if (!part.effect.Contains ("Wait")) {
                    user.activeMoveIfParts.RemoveAt (0);

                    if (user.hpLeft > 0 && user.activeMoveIfParts.Count () > 0) {
                        if (user.activeMoveIfParts [0].GetType () == typeof (Move.Part)) {
                            Move.Part nextPart = (Move.Part) user.activeMoveIfParts [0];
                            
                            if (!nextPart.effect.Contains ("Wait")) {
                                ProcessMoveIfPart (user, other);
                            }
                        } else if (user.activeMoveIfParts [0].GetType () == typeof (Move.MoveCondition)) {
                            ProcessMoveIfPart (user, other);
                        }
                    }
                } else if (part.effectNum == 0) {
                    user.activeMoveIfParts.RemoveAt (0);
                }

                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
                DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
            }
        }

        /// <summary>
        /// Removes the unit's box and typewrites the faint text
        /// </summary>
        /// <param name="isPlayer">Whether the faintee is the player or not</param>
        /// <param name="isOtherAlive">Whether the other unit is alive or not</param>
        private void FaintUnit (bool isPlayer, bool isOtherAlive) {
            Clear (ClearType.AboveDialog);                         //Clear the above dialog

            if (isPlayer) {                                        //If the faintee is the player
                if (isOtherAlive) {                                    //If the enemy is alive
                    DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);    //Draw the enemy's box
                }

                TypeWrite (BuildText (Data._battleInfo.yourUnitDiedText, _player), ConsoleColor.DarkYellow, true);
            } else {                                               //If the faintee is the enemy
                if (isOtherAlive) {                                    //If the player is alive
                    DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);   //Draw the player's box
                }

                if (_trainer.name.Length == 0) {                       //If the enemy is a trainer
                    TypeWrite (_enemy.diedText, ConsoleColor.DarkYellow, true);
                } else {                                               //If the enemy is not a trainer
                    TypeWrite (BuildText (_trainer.unitDiedText, _enemy), ConsoleColor.DarkYellow, true);
                }
            }
        }

        /// <summary>
        /// Handles the exp gaining and unit choosing and whether the fight is over
        /// </summary>
        /// <param name="firstUnit">The first unit to die</param>
        /// <param name="firstMove">The first unit's move</param>
        /// <param name="secondUnit">The second unit to die</param>
        /// <param name="secondMove">The second unit's move</param>
        private void HandleDead (ref Unit firstToDie, ref Move firstMove, ref Unit secondToDie, ref Move secondMove) {
            if (firstToDie.Equals (_enemy)) {         //If the first to die is the enemy
                bool enemyDead = (firstToDie.hpLeft <= 0) ? true : false;
                bool playerDead = (secondToDie.hpLeft <= 0) ? true : false;
                bool enemyMustPick = false;
                bool playerMustPick = false;

                if (enemyDead) {                          //If the enemy is dead
                    firstMove = new Move ();                  //Set the enemy's move to nothing
                    firstMove.ifParts = new List<Move.IfPart> { };
                    firstToDie.ailment.turns = 0;             //Set the enemy's ailment to nothing
                                                              //If it's a trainer and he has more units
                    if (_trainer.name.Length > 0 && _trainer.units.Exists (p => p.hpLeft > 0)) {
                        enemyMustPick = true;                     //The trainer must pick another unit
                    } else {                                  //If it's not a trainer
                        _fightOver = true;                        //The fight is over
                        GameFile._lastBattleWon = true;
                    }
                }

                if (playerDead) {                         //If the player is dead
                    secondMove = new Move ();                 //Set the player's move to nithing
                    secondMove.ifParts = new List<Move.IfPart> { };
                    secondToDie.ailment.turns = 0;            //Set the player's ailment to nothing

                    if (!UnitMaster.AllDead ()) {             //If the player has more units
                        playerMustPick = true;                    //The player must pick another unit
                    } else {                                  //If the player has no more units
                        _fightOver = true;                        //The fight is over
                        GameFile._lastBattleWon = false;
                    }
                } else {                                  //If the player is not dead
                    if (enemyDead) {                          //If the enemy is dead
                        if (_trainer.name.Length == 0) {      //If it's not a trainer
                            _fightOver = true;                    //The fight is over
                            GameFile._lastBattleWon = true;
                        }
                                                              //Gain exp
                        int exp = (int) (firstToDie.enemyBaseExp * Math.Pow ((double) Data._unitInfo.enemyExpMult, firstToDie.level - 1));
                        TypeWrite (BuildText (Data._unitInfo.gainEXPText, _player).Replace ("#", exp.ToString ()), ConsoleColor.DarkYellow, true);
                        IncreaseEXP (_player, exp);
                    }
                }

                if (enemyMustPick) {                     //If the enemy has to pick another unit
                    SendOutUnit (false, true, _trainer.units.Find (p => p.hpLeft > 0));
                    firstToDie = _enemy;
                }

                if (playerMustPick) {                    //If the player has to pick another unit
                    secondToDie = ChooseNextUnit ();
                }
            } else if (firstToDie.Equals (_player)) {//If the first to die is the player
                bool playerDead = (firstToDie.hpLeft <= 0) ? true : false;
                bool enemyDead = (secondToDie.hpLeft <= 0) ? true : false;
                bool playerMustPick = false;
                bool enemyMustPick = false;

                if (playerDead) {                        //If the player is dead
                    firstMove = new Move ();                 //Set the player's move to nothing
                    firstMove.ifParts = new List<Move.IfPart> { };
                    firstToDie.ailment.turns = 0;            //Set the player's ailment to nothing

                    if (!UnitMaster.AllDead ()) {            //If the player has more units
                        playerMustPick = true;                   //The player must pick another unit
                    } else {                                 //If the player has no more units
                        _fightOver = true;                       //The fight is over
                        GameFile._lastBattleWon = false;
                    }
                } else {                                 //If the player is not dead
                    if (enemyDead) {                         //If the enemy is dead, gain exp
                        int exp = (int) (firstToDie.enemyBaseExp * Math.Pow ((double) Data._unitInfo.enemyExpMult, firstToDie.level - 1));
                        TypeWrite (BuildText (Data._unitInfo.gainEXPText, _player).Replace ("#", exp.ToString ()), ConsoleColor.DarkYellow, false, false, true);
                        IncreaseEXP (_player, exp);
                    }
                }

                if (enemyDead) {                         //If the enemy is dead
                    if (_trainer.name.Length == 0) {         //If it's not a trainer
                        playerMustPick = false;                  //The player will not pick another unit
                        _fightOver = true;                       //The fight is over
                        GameFile._lastBattleWon = true;
                    } else {                                 //If it's a trainer
                        if (_trainer.units.FindIndex (p => p.hpLeft > 0) != -1) {//If the trainer has more units
                            enemyMustPick = true;                    //The enemy must pick another unit
                        } else {
                            _fightOver = true;

                            if (!UnitMaster.AllDead ()) {
                                GameFile._lastBattleWon = true;
                            }
                        }
                    }
                }

                if (playerMustPick) {                    //If the player must pick another unit
                    firstToDie = ChooseNextUnit ();
                }

                if (enemyMustPick) {                     //If the enemy must pick another unit
                    SendOutUnit (false, true, _trainer.units.Find (p => p.hpLeft > 0));
                    secondToDie = _enemy;
                }
            }
        }

        private Unit ChooseNextUnit () {
            MenuUnitsScreen mps = new MenuUnitsScreen (0, BuildText (Data._battleInfo.replaceUnitText, _player), "");

            for (int i = 0; i < GameFile._units.Count (); i++) {
                mps._selectionAbles [i] = (GameFile._units [i].hpLeft <= 0 || GameFile._units [i].Equals (_player)) ? false : true;
            }

            int selection = mps.GetSelection (false, new List<string> { "Select", "Stats", });
            DrawScreenTitle ("Battle");
            Clear (ClearType.Both);

            if (_enemy.hpLeft > 0) {
                DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
            }

            SendOutUnit (true, false, GameFile._units [selection]);
            return GameFile._units [selection];
        }

        private string GetMovePartString (string s, Unit owner) {
            if (owner.Equals (_player)) {
                s = s.Replace ("@", _player.name);
                s = s.Replace ("#", _enemy.name);
            } else {
                s = s.Replace ("@", _enemy.name);
                s = s.Replace ("#", _player.name);
            }

            return s;
        }

        private string BuildText (string s, Unit owner) {
            s = s.Replace ("@", owner.name);
            return s;
        }

        private string BuildText (string s, Unit user, Unit target) {
            s = s.Replace ("@", user.name);
            s = s.Replace ("#", target.name);
            return s;
        }

        private void DecreaseHP (Unit unit, int damage) {
            UnitMaster.DecreaseHPEvent -= onDecreaseHP;
            UnitMaster.DecreaseHPEvent += onDecreaseHP;
            UnitMaster.DecreaseHP (unit, damage, false);
            
            if (unit.Equals (_player)) {
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
            } else if (unit.Equals (_enemy)) {
                DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
            }
        }

        private void onDecreaseHP (Unit unit) {
            if (unit.Equals (_player)) {
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkRed);
            } else if (unit.Equals (_enemy)) {
                DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkRed);
            }
        }

        private void IncreaseHP (Unit unit, int amount) {
            UnitMaster.IncreaseHPEvent -= onIncreaseHP;
            UnitMaster.IncreaseHPEvent += onIncreaseHP;
            UnitMaster.IncreaseHP (unit, amount, false);

            if (_selState == SelectionState.Fight) {
                if (unit.Equals (_player)) {
                    DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
                } else if (unit.Equals (_enemy)) {
                    DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
                }
            }
        }

        private void onIncreaseHP (Unit unit) {
            if (_selState == SelectionState.Fight) {
                if (unit.Equals (_player)) {
                    DrawScreenTitle ("Battle");
                    Clear (ClearType.Both);
                    DrawBox (BoxToDraw.Player, ConsoleColor.DarkRed);
                    DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkYellow);
                } else if (unit.Equals (_enemy)) {
                    DrawBox (BoxToDraw.Enemy, ConsoleColor.DarkRed);
                }
            } else if (_selState == SelectionState.Bag) {
                if (GameFile._units.IndexOf (unit) != -1) {
                    new MenuUnitsScreen (GameFile._units.IndexOf (unit));
                }
            }
        }

        private string DrawHP (decimal hpLeft, int hp) {
            string s = new string ('▓', (int) (hpLeft / ((decimal) hp / 10)));
            decimal blockPercent = hpLeft % ((decimal) hp / 10) / ((decimal) hp / 10);
            
            if (s.Length < 10) {
                if (blockPercent > 0.66m) {
                    s += "▓";
                } else if (blockPercent > 0.33m) {
                    s += "▒";
                } else if (blockPercent > 0) {
                    s += "░";
                }
            }
            
            s += new string (' ', 10 - s.Length);
            return s;
        }

        private void IncreaseEXP (Unit unit, int exp) {
            UnitMaster.IncreaseEXPEvent -= onIncreaseEXP;
            UnitMaster.IncreaseEXPEvent += onIncreaseEXP;
            UnitMaster.LevelUpEvent -= onLevelUp;
            UnitMaster.LevelUpEvent += onLevelUp;
            UnitMaster.IncreaseEXP (unit, exp, _selState == SelectionState.Bag);
        }

        private void onIncreaseEXP (Unit unit) {
            if (_selState == SelectionState.Fight) {
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
            }
        }

        private void onLevelUp (Unit unit) {
            if (_selState == SelectionState.Fight) {
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
                TypeWrite (BuildText (Data._unitInfo.levelUpText, _player), ConsoleColor.DarkYellow, true);
                DrawStatBox (unit, ConsoleColor.DarkYellow);

                Clear (ClearType.AboveDialog);
                DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
            } else if (_selState == SelectionState.Bag) {
                new MenuUnitsScreen (GameFile._units.IndexOf (unit));
                TypeWrite (BuildText (Data._unitInfo.levelUpText, _player), ConsoleColor.DarkYellow, true);
            }

            for (int i = 0; i < unit.nativeMoveLevels.Count (); i++) {
                if (unit.nativeMoveLevels [i] == unit.level) {
                    UnitMaster.LearnMove (this, unit, unit.nativeMoves [i]);
                }
            }

            DrawBox (BoxToDraw.Player, ConsoleColor.DarkYellow);
        }

        private string DrawEXP (decimal exp, long expNext, int level) {
            int expBase = Data._unitInfo.expNextBase;
            decimal expMult = Data._unitInfo.expNextMult;
            decimal expPrev = expBase * (decimal) (1 - Math.Pow ((double) expMult, level - 1)) / (1 - expMult);
            string s = new string ('▓', (int) ((exp - expPrev) / ((expNext - expPrev) / 10)));
            decimal blockPercent = (exp - expPrev) % ((decimal) (expNext - expPrev) / 10) / ((decimal) (expNext - expPrev) / 10);
            
            if (s.Length < 10) {
                if (blockPercent > 0.66m) {
                    s += "▓";
                } else if (blockPercent > 0.33m) {
                    s += "▒";
                } else if (blockPercent > 0) {
                    s += "░";
                }
            }

            s += new string (' ', 10 - s.Length);
            return s;
        }

        private void DrawBox (BoxToDraw boxToDraw, ConsoleColor color) {
            if (boxToDraw == BoxToDraw.Player) {
                int line = 2 + Data._generalInfo.aboveHeight - 6;
                string firstLine = _player.name + " " + Data._unitInfo.levelShort + " " + _player.level.ToString ();
                string secondLine = ((_player.ailment.turns == 0) ? "   " : _player.ailment.tag) + " " + Data._unitInfo.hpShort + DrawHP (_player.hpLeft, _player.stats ["HP"]);
                string space = "";

                if (firstLine.Length >= secondLine.Length) {
                    space = new string (' ', Data._generalInfo.width - 1 - firstLine.Length - 1);
                    Write ("  " + space + "╔" + new string ('═', firstLine.Length) + "╗", line, color);
                    Write ("  " + space + "║" + firstLine + "║", line + 1, color);
                    Write ("  " + space + "║" + ((_player.ailment.turns == 0) ? "   " : _player.ailment.tag) + new string (' ', firstLine.Length - secondLine.Length) + " " + Data._unitInfo.hpShort + DrawHP (_player.hpLeft, _player.stats ["HP"]) + "║", line + 2, color);
                    Write ("  " + space + "║" + new string (' ', firstLine.Length - ((int) _player.hpLeft).ToString ().Length - 1 - _player.stats ["HP"].ToString ().Length) + ((int) _player.hpLeft).ToString () + "/" + _player.stats ["HP"].ToString () + "║", line + 3, color);
                    Write ("  " + space + "╚" + new string ('═', firstLine.Length - 3 - 10 - 1 - 1) + "╗EXP" + DrawEXP (_player.exp, _player.expNext, _player.level) + "╔╝", line + 4, color);
                    Write ("  " + space + new string (' ', firstLine.Length - 13 - 1) + "╚═════════════╝ ", line + 5, color);
                } else {
                    space = new string (' ', Data._generalInfo.width - 1 - secondLine.Length - 1);
                    Write ("  " + space + "╔" + new string ('═', secondLine.Length) + "╗", line, color);
                    Write ("  " + space + "║" + _player.name + new string (' ', secondLine.Length - firstLine.Length) + " " + Data._unitInfo.levelShort + " " + _player.level.ToString () + "║", line + 1, color);
                    Write ("  " + space + "║" + secondLine + "║", line + 2, color);
                    Write ("  " + space + "║" + new string (' ', secondLine.Length - ((int) _player.hpLeft).ToString ().Length - 1 - _player.stats ["HP"].ToString ().Length) + ((int) _player.hpLeft).ToString () + "/" + _player.stats ["HP"].ToString () + "║", line + 3, color);
                    Write ("  " + space + "╚" + new string ('═', secondLine.Length - 3 - 10 - 1 - 1) + "╗EXP" + DrawEXP (_player.exp, _player.expNext, _player.level) + "╔╝", line + 4, color);
                    Write ("  " + space + new string (' ', secondLine.Length - 13 - 1) + "╚═════════════╝ ", line + 5, color);
                }
            } else if (boxToDraw == BoxToDraw.Enemy) {
                string firstLine = _enemy.name + " " + Data._unitInfo.levelShort + " " + _enemy.level.ToString ();
                string secondLine = ((_enemy.ailment.turns == 0) ? "   " : _enemy.ailment.tag) + " " + Data._unitInfo.hpShort + DrawHP (_enemy.hpLeft, _enemy.stats ["HP"]);

                if (firstLine.Length >= secondLine.Length) {
                    Write ("  " + "╔" + new string ('═', firstLine.Length) + "╗", 2, color);
                    Write ("  " + "║" + firstLine + "║", 3, color);
                    Write ("  " + "║" + ((_enemy.ailment.turns == 0) ? "PSN" : _enemy.ailment.tag) + new string (' ', firstLine.Length - secondLine.Length) + " " + Data._unitInfo.hpShort + DrawHP (_enemy.hpLeft, _enemy.stats ["HP"]) + "║", 4, color);
                    Write ("  " + "╚" + new string ('═', firstLine.Length) + "╝", 5, color);
                } else {
                    Write ("  " + "╔" + new string ('═', secondLine.Length) + "╗", 2, color);
                    Write ("  " + "║" + _enemy.name + new string (' ', secondLine.Length - firstLine.Length) + " " + Data._unitInfo.levelShort + " " + _enemy.level.ToString () + "║", 3, color);
                    Write ("  " + "║" + secondLine + "║", 4, color);
                    Write ("  " + "╚" + new string ('═', secondLine.Length) + "╝", 5, color);
                }
            }
        }

        private void DrawStatBox (Unit unit, ConsoleColor color) {
            List <string> strings = new List <string> {};
            strings.Add (unit.name);
            strings.Add (Data._unitInfo.statNames ["Level"] + ": " + (unit.level - 1).ToString ());

            foreach (KeyValuePair <string, int> pair in unit.oldStats) {
                strings.Add (Data._unitInfo.statNames [pair.Key] + ": " + (pair.Value - unit.statGrowths [pair.Key]).ToString ());
            }

            List <int> adds = new List <int> {1};
            
            foreach (KeyValuePair <string, int> pair in unit.statGrowths) {
                adds.Add (pair.Value);
            }

            int largest = strings [0].Length;

            for (int i = 1; i < strings.Count (); i++) {
                if ((strings [i] + "+" + adds [i - 1].ToString ()).Length > largest) {
                    largest = (strings [i] + "+" + adds [i - 1].ToString ()).Length;
                }
            }

            int line = 2 + Data._generalInfo.aboveHeight - 10;
            Console.SetCursorPosition (0, line);
            Console.ForegroundColor = color;
            Console.Write ("╔" + new string ('═', largest) + "╗");
            Console.SetCursorPosition (0, line + 1);
            Console.Write ("║" + strings [0] + new string (' ', largest - strings [0].Length) + "║");

            for (int i = 1; i < strings.Count (); i++) {
                Console.SetCursorPosition (0, line + i + 1);
                Console.ForegroundColor = color;
                Console.Write ("║" + strings [i] + new string (' ', largest - strings [i].Length) + "║");
            }

            Console.SetCursorPosition (0, line + strings.Count () + 1);
            Console.ForegroundColor = color;
            Console.Write ("╚" + new string ('═', largest) + "╝");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;

            for (int i = 1; i < strings.Count (); i++) {
                Console.SetCursorPosition (1 + strings [i].Length, line + i + 1);
                Console.Write ("+" + adds [i - 1].ToString ());
            }

            RemoveExcessKeys ();
            Console.ReadKey (true);
        }

        private void RemoveExcessKeys () {
            while (Console.KeyAvailable) {
                Console.ReadKey (true);
            }
        }
    }
}