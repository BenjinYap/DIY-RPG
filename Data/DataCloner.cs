using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DIY_RPG {

    public class DataCloner {

        public static Unit CloneUnit (int id) {
            Unit mold = Data._unitInfo.units [id];
            Unit p = new Unit ();
            p.id = id;
            p.name = mold.name;
            p.desc = mold.desc;
            p.moves = new List<Move> { };
            p.nativeMoves = mold.nativeMoves;
            p.nativeMoveLevels = mold.nativeMoveLevels;
            p.invincible = false;
            p.activeMoveIfParts = new List<Move.IfPart> { };
            p.stats = new Dictionary <string,int> {};

            foreach (KeyValuePair <string, int> pair in mold.stats) {
                p.stats [pair.Key] = pair.Value;
            }

            p.oldStats = new Dictionary<string, int> { };
            p.statGrowths = mold.statGrowths;
            p.ailment = new Ailment ();
            p.enemyBaseExp = mold.enemyBaseExp;
            return p;
        }

        public static Move CloneMove (int id) {
            Move mold = Data._moveInfo.moves [id];
            Move m = new Move ();
            m.id = id;
            m.name = mold.name;
            m.desc = mold.desc;
            
            return new Move ();
        }

        public static Move CloneMove (Move mold) {
            Move m = mold;
            m.ifParts = new List<Move.IfPart> { };

            if (mold.ifParts != null) {
                foreach (Move.IfPart part in mold.ifParts) {
                    m.ifParts.Add (part.Clone ());
                }
            }

            return m;
        }
    }
}
