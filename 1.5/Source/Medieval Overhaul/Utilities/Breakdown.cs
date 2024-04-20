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
    public class Breakdown
    {
        public Thing FindComponent(Pawn pawn, Thing thing)
        {
            if (thing.def.techLevel == TechLevel.Neolithic || thing.def.techLevel == TechLevel.Medieval)
            {
                return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(MedievalOverhaulDefOf.DankPyon_ComponentBasic), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false, false, false), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
            }
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.ComponentIndustrial), PathEndMode.InteractionCell, TraverseParms.For(pawn, pawn.NormalMaxDanger(), TraverseMode.ByPawn, false, false, false), 9999f, (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
