using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using StandaloneSettlementPreference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using MedievalOverhaul.StandaloneSettlementPreference;

namespace StandaloneSettlementPreference
{
    public static class SettlementPreferencePatch
    {
        [HarmonyPatch(typeof(TileFinder))]
        [HarmonyPatch("RandomSettlementTileFor")]
        public static class TileFinder_RandomSettlementTileFor_Patch
        {
            [HarmonyPrefix]
            public static bool SettlementPatch(ref int __result, Faction faction) => !SettlementPreference_ModSettings.Enable_SettlementPreference || faction == null || SettlementPreference.Get((Def)faction.def) == null || !Rand.Chance(SettlementPreference.Get((Def)faction.def).chance) || SettlementPreferenceUtility.GetTileID(faction, out __result);
        }
    }
}
