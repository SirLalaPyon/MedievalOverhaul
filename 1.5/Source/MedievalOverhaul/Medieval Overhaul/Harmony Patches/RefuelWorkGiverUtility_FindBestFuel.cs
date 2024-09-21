using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace MedievalOverhaul.Patches
{
    //[HarmonyPatch(typeof(RefuelWorkGiverUtility))]
    //[HarmonyPatch("FindBestFuel")]
    //public class RefuelWorkGiverUtility_FindBestFuel
    //{
    //    [HarmonyPrefix]
    //    public static bool Prefix(Pawn pawn, Thing refuelable, ref Thing __result)
    //    {
    //        if (Utility.LWMFuelFilterIsEnabled || !(refuelable is Building))
    //        {
    //            return true;
    //        }
    //        //CompRefuelable compRefuelable = refuelable.TryGetComp<CompRefuelable>();
    //        var compRefuelable = refuelable.TryGetComp<CompRefuelable>()?.HasFuel;
    //        CompStoreFuelThing compStorage = refuelable.TryGetComp<CompStoreFuelThing>();
    //        if (compRefuelable != null && compStorage != null)
    //        {
    //            ThingFilter filter = compStorage.AllowedFuelFilter;
    //            if (compRefuelable == true && compStorage.fuelUsed != null)
    //            {
    //                ThingDef fuelUsed = compStorage.fuelUsed;
    //                if (pawn.Map.listerThings.AnyThingWithDef(fuelUsed))
    //                {
    //                    Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
    //                    __result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(fuelUsed), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
    //                    return false;
    //                }
    //                __result = null;
    //                return false;
    //            }
    //            if (Utility.FilterItemExists(filter, pawn))
    //            {
    //                Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && filter.Allows(x);
    //                IEnumerable<Thing> searchSet = pawn.Map.listerThings.ThingsMatchingFilter(filter);
    //                __result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn), 9999f, validator, searchSet, 0, -1, false, RegionType.Set_Passable, false);
    //            }
    //            return false;
    //        }
    //        return true;
    //    }
    //}
}