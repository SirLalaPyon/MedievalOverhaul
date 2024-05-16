using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class HediffComp_Daze : HediffComp
    {
        public override bool CompShouldRemove
        {
            get
            {
                Pawn pawn = this.Pawn;
                if (pawn.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_DazeImmune))
                {
                    return true;
                }
                return false;
            }
        }
    }
}