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
    //[HarmonyPatch(typeof(CompRefuelable), "Refuel", new System.Type[] { typeof(List<Thing>) })]
    //public class CompRefuelable_StoreFuel_Patch
    //{
    //    [HarmonyPrefix]
    //    public static bool StoreFuel(CompRefuelable __instance, List<Thing> fuelThings)
    //    {
    //        if (Utility.LWMFuelFilterIsEnabled || fuelThings.NullOrEmpty<Thing>() || __instance.parent.GetComp<CompStoreFuelThing>() == null)
    //            return true;
    //        ThingDef def = fuelThings.Last<Thing>().def;
    //        __instance.parent.GetComp<CompStoreFuelThing>().fuelUsed = def;
    //        return true;
    //    }
    //}
}
