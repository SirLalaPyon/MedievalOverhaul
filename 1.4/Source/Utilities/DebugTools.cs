using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public static class DebugTools
    {
        [DebugAction("MedievalOverhaul", "MOTest", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]

        public static void MOTest(Pawn p)
        {
            UI.MouseCell();
            Log.Message(p.LabelShort);
        }
    }
}
