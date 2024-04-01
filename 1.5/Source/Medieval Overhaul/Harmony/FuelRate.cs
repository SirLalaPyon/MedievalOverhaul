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
    [HarmonyPatch(typeof(CompRefuelable), "ConsumptionRatePerTick", MethodType.Getter)]
    public class FuelRate_Patch
    {
        [HarmonyPostfix]
        public static void FuelRate(CompRefuelable __instance, ref float __result)
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled)
                return;
            float num = 1f;
            CompStoreFuelThing comp = __instance.parent.GetComp<CompStoreFuelThing>();
            if (comp != null && __instance.HasFuel && comp.fuelUsed != null)
            {
                ThingDef fuelUsed = __instance.parent.GetComp<CompStoreFuelThing>().fuelUsed;
                if (fuelUsed.GetCompProperties<CompProperties_FuelRate>() != null)
                    num = fuelUsed.GetCompProperties<CompProperties_FuelRate>().rate;
            }
            __result = __instance.Props.fuelConsumptionRate / (60000f * num);
        }
    }
}
