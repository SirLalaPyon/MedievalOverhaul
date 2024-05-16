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
    public class WorkGiver_OperateQuest : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(this.ScannerDef);
            }
        }

        public ThingDef ScannerDef
        {
            get
            {
                return this.def.scannerDef;
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            foreach (Thing thing in pawn.Map.listerThings.ThingsOfDef(this.ScannerDef))
            {
                CompQuestFinder questFinder;
                if (thing.Faction == pawn.Faction && thing.TryGetComp(out questFinder) && questFinder.CanUseNow)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_QuestScanner building;
            return t.Faction == pawn.Faction && (building = (t as Building_QuestScanner)) != null && !building.IsForbidden(pawn) && pawn.CanReserve(building, 1, -1, null, forced) && building.TryGetComp<CompQuestFinder>().CanUseNow && !building.IsBurning();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(MedievalOverhaulDefOf.DankPyon_OperateQuest, t, 1500, true);
        }
    }
}
