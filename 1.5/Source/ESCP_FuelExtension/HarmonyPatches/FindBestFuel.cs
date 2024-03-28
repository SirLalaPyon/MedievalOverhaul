using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace ESCP_FuelExtension
{
    [HarmonyPatch(typeof(RefuelWorkGiverUtility))]
    [HarmonyPatch("FindBestFuel")]
    public class FindBestFuelPatch
    {
        [HarmonyPostfix]
        public static void FindBestFuel(Pawn pawn, Thing refuelable, ref Thing __result)
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled || !(refuelable is Building))
                return;
            Building building = refuelable as Building;
            CompRefuelable comp1 = building.GetComp<CompRefuelable>();
            if (comp1 != null)
            {
                CompStoreFuelThing comp2 = building.GetComp<CompStoreFuelThing>();
                if (comp2 != null)
                {
                    if (comp1.HasFuel && comp2.fuelUsed != null)
                    {
                        ThingDef fuelUsed = comp2.fuelUsed;
                        if (comp2.AllowedFuelFilter.Allows(fuelUsed))
                        {
                            Thing specificFuel = Utility.FindSpecificFuel(pawn, fuelUsed);
                            if (specificFuel != null)
                                __result = specificFuel;
                        }
                        else if (!comp2.AllowedFuelFilter.Allows(fuelUsed))
                            __result = (Thing)null;
                    }
                    else
                    {
                        List<ThingDef> list = comp2.AllowedFuelFilter.AllowedThingDefs.ToList<ThingDef>();
                        if (!list.NullOrEmpty<ThingDef>())
                        {
                            Thing specificClosestFuel = Utility.FindSpecificClosestFuel(pawn, list);
                            if (specificClosestFuel != null)
                            {
                                __result = specificClosestFuel;
                                return;
                            }
                        }
                        __result = (Thing)null;
                    }
                }
            }
        }
    }
}