using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{ 
    [HarmonyPatch(typeof(StatWorker_MarketValue), "CalculableRecipe")]
    public static class CalculableRecipe_Patch
    {
        private static bool Prefix(BuildableDef def)
        {
            if (def is ThingDef thingDef && thingDef.IsChunk())
            {
                return false;
            }
            return true;
        }
    }
}