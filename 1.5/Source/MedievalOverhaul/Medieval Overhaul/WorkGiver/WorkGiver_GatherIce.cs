using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace MedievalOverhaul
{
    
    public class WorkGiver_GatherIce : WorkGiver_ConstructAffectFloor
    {
        public override DesignationDef DesDef => MedievalOverhaulDefOf.DankPyon_GatherIce;

        public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false) => new Job(MedievalOverhaulDefOf.DankPyon_JobGatherIce, cell);
    }
}
