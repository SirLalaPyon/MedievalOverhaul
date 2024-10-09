using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(RitualObligationTargetWorker_CampfireParty), nameof(RitualObligationTargetWorker_CampfireParty.CanUseTargetInternal))]
    public static class RitualObligationTargetWorker_CampfireParty_CanUseTargetInternal_Patch
    {
        public static RitualTargetUseReport Postfix(RitualTargetUseReport __result, TargetInfo target)
        {
                Thing thing = target.Thing;
                CompRefuelableCustom compRefuelableCustom = thing.TryGetComp<CompRefuelableCustom>();
                Log.Message(compRefuelableCustom);
                if (compRefuelableCustom != null && !compRefuelableCustom.HasFuel)
                {
                    return "RitualTargetCampfireNoFuel".Translate();
                }
            return __result;
        }

    }
}
