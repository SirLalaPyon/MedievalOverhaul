using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(ResearchProjectDef), "CanBeResearchedAt")]
    public static class ResearchProjectDef_CanBeResearchedAt
    {
        public static void Postfix(Building_ResearchBench bench, ref bool __result)
        {
            if (__result)
            {
                var fuelComp = bench.GetComp<CompRefuelable>();
                if (fuelComp != null && !fuelComp.HasFuel)
                {
                    __result = false;
                }
            }
        }
    }
}
