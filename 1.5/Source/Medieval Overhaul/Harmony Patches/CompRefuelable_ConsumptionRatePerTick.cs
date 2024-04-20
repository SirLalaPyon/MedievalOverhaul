using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(CompRefuelable), "ConsumptionRatePerTick", MethodType.Getter)]
    public class CompRefuelable_ConsumptionRatePerTick
    {
        [HarmonyPostfix]
        public static void Postfix(CompRefuelable __instance, ref float __result)
        {
            if (Utility.LWMFuelFilterIsEnabled)
                return;
            float num = 1f;
            var thingDef = __instance.parent.GetComp<CompStoreFuelThing>()?.fuelUsed;
            if (__instance.HasFuel && thingDef != null)
            {
                ThingDef thingFuel = thingDef;
                num = thingFuel.GetModExtension<FuelValueProperty>()?.fuelValue ?? num;
            }
            __result = __instance.Props.fuelConsumptionRate / (60000f * num);
        }
    }
}