using ESCP_FuelExtension;
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
    [HarmonyPatch(typeof(CompRefuelable), "Refuel", new System.Type[] { typeof(List<Thing>) })]
    public class StoreFuel_Patch
    {
        [HarmonyPrefix]
        public static bool StoreFuel(CompRefuelable __instance, List<Thing> fuelThings)
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled || fuelThings.NullOrEmpty<Thing>() || __instance.parent.GetComp<CompStoreFuelThing>() == null)
                return true;
            ThingDef def = fuelThings.Last<Thing>().def;
            __instance.parent.GetComp<CompStoreFuelThing>().fuelUsed = def;
            return true;
        }
    }
}
