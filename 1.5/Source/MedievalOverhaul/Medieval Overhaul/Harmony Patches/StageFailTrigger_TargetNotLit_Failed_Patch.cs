using HarmonyLib;
using ProcessorFramework;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(StageFailTrigger_TargetNotLit), nameof(StageFailTrigger_TargetNotLit.Failed))]

    public static class StageFailTrigger_TargetNotLit_Failed_Patch
    {
        public static bool Prefix(StageFailTrigger_TargetNotLit __instance, ref bool __result, LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
        {
            if (__instance.onlyIfTargetIsOfDef != null && ritual.selectedTarget.Thing?.def != __instance.onlyIfTargetIsOfDef)
            {
                __result = false;
                return false;
            }
            CompRefuelableCustom compRefuelableCustom = ritual.selectedTarget.Thing?.TryGetComp<CompRefuelableCustom>();
            if (compRefuelableCustom != null)
            {
                __result = !compRefuelableCustom.HasFuel;
                return false;
            }
            return true;
        }
    }
}
