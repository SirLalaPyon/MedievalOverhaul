using HarmonyLib;
using RimWorld;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(StaggerHandler), nameof(StaggerHandler.StaggerFor))]
    public class StaggerHandler_StaggerFor
    {

        [HarmonyPostfix]
        public static bool Prefix(ref bool __result, StaggerHandler __instance)
        {
            if (__instance != null)
            {
                if (__instance.parent.health.hediffSet.HasHediff(MedievalOverhaulDefOf.DankPyon_StunImmune))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
