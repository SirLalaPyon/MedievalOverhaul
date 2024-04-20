using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using ProcessorFramework;
using RimWorld;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(CompProcessor), "TakeOutProduct")]
    public static class CompProcessor_TakeOutProduct
    {
        public static bool Prefix(CompProcessor __instance, ref Thing __result, ActiveProcess activeProcess)
        {
            var extension = activeProcess.processDef.GetModExtension<ProcessorExtension>();
            if (extension != null)
            {
                if (extension.outputOnlyButcherProduct)
                {
                    if (activeProcess.ingredientThings != null)
                    {
                        foreach (var thing in activeProcess.ingredientThings)
                        {
                            if (thing != null)
                            {
                                if (thing.def.butcherProducts?.Any() ?? false)
                                {
                                    var thingDefCount = thing.def.butcherProducts.FirstOrDefault();
                                    __result = TakeOutButcherProduct(__instance, thingDefCount, activeProcess);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static Thing TakeOutButcherProduct(CompProcessor __instance, ThingDefCountClass thingDefCount, ActiveProcess activeProcess)
        {
            Thing thing = null;
            if (!activeProcess.Ruined)
            {
                thing = ThingMaker.MakeThing(thingDefCount.thingDef);
                thing.stackCount = GenMath.RoundRandom(activeProcess.ingredientCount * activeProcess.processDef.efficiency);
                Log.Message(thing.stackCount);
            }
            foreach (Thing ingredientThing2 in activeProcess.ingredientThings)
            {
                __instance.innerContainer.Remove(ingredientThing2);
                ingredientThing2.Destroy();
            }
            __instance.activeProcesses.Remove(activeProcess);
            if (__instance.Empty)
            {
                __instance.GraphicChange(toEmpty: true);
            }
            if (!__instance.activeProcesses.Any((ActiveProcess x) => x.processDef.usesQuality))
            {
                __instance.emptyNow = false;
            }
            return thing;
        }
    }
}
