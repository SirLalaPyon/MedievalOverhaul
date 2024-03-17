using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ESCP_FuelExtension
{
    [HarmonyPatch(typeof(CompRefuelable), "CompInspectStringExtra")]
    public class InspecStringPatch
    {
        [HarmonyPrefix]
        public static bool InspectString(
          CompRefuelable __instance,
          ref string __result,
          ref float ___fuel)
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled)
                return true;
            CompStoreFuelThing comp = __instance.parent.GetComp<CompStoreFuelThing>();
            if (comp != null && __instance.HasFuel && comp.fuelUsed != null && !__instance.Props.consumeFuelOnlyWhenUsed && __instance.HasFuel)
            {
                float num = 1f;
                ThingDef fuelUsed = __instance.parent.GetComp<CompStoreFuelThing>().fuelUsed;
                if (fuelUsed.GetCompProperties<CompProperties_FuelRate>() != null)
                    num = fuelUsed.GetCompProperties<CompProperties_FuelRate>().rate;
                if (fuelUsed.GetCompProperties<CompProperties_FuelRate>() != null)
                {
                    string str = __instance.Props.FuelLabel + ": " + ___fuel.ToStringDecimalIfSmall() + " / " + __instance.Props.fuelCapacity.ToStringDecimalIfSmall();
                    if (!__instance.Props.consumeFuelOnlyWhenUsed && __instance.HasFuel)
                    {
                        int numTicks = (int)((double)___fuel / (double)__instance.Props.fuelConsumptionRate * 60000.0 * (double)num);
                        str = str + " (" + numTicks.ToStringTicksToPeriod() + ")";
                    }
                    if (!__instance.HasFuel && !__instance.Props.outOfFuelMessage.NullOrEmpty())
                        str += string.Format("\n{0} ({1}x {2})", (object)__instance.Props.outOfFuelMessage, (object)__instance.GetFuelCountToFullyRefuel(), (object)__instance.Props.fuelFilter.AnyAllowedDef.label);
                    if (__instance.Props.targetFuelLevelConfigurable)
                        str = (string)(str + ("\n" + "ConfiguredTargetFuelLevel".Translate((NamedArgument)__instance.TargetFuelLevel.ToStringDecimalIfSmall())));
                    __result = str;
                    return false;
                }
            }
            return true;
        }
    }
}
