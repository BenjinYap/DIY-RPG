using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIY_RPG {

    public class CreditsScreen:SelectionScreen {

        public CreditsScreen () {
            Clear (ClearType.Both);
            DrawScreenTitle ("Credits");
            
            for (int i = 0; i < Data._generalInfo.creditLines.Count (); i++) {
                List <string> line = Helper.WrapText (Data._generalInfo.creditLines [i], Data._generalInfo.width, false);
                AddSelection (line, 0, true);
            }

            int selection = 0;

            while (selection != -1) {
                selection = GetSelection (true, selection);
            }
        }
    }
}
