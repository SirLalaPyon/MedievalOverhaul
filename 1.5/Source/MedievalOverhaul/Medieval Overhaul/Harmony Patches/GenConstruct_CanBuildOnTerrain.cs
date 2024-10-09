using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(GenConstruct), "CanBuildOnTerrain")]
    public static class GenConstruct_CanBuildOnTerrain
    {
        public static void Postfix(ref bool __result, BuildableDef entDef, IntVec3 c, Map map)
        {
            if (entDef == MedievalOverhaulDefOf.DankPyon_PlowedSoil && !c.GetTerrain(map).IsSoil)
            {
                __result = false;
            }
        }
    }
}