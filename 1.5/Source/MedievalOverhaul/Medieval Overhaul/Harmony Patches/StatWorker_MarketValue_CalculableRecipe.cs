using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{ 
    [HarmonyPatch(typeof(StatWorker_MarketValue), "CalculableRecipe")]
    public static class StatWorker_MarketValue_CalculableRecipe
    {
        public static bool Prefix(BuildableDef def)
        {
            if (def is ThingDef thingDef && thingDef.IsChunk())
            {
                return false;
            }
            return true;
        }
    }
}