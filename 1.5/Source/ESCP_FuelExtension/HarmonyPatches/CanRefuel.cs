using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace ESCP_FuelExtension
{
    [HarmonyPatch(typeof(RefuelWorkGiverUtility), "CanRefuel")]
    public class CanRefuelPatch
    {
        [HarmonyPostfix]
        public static void CanRefuel(Pawn pawn, Thing t, ref bool __result)
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled || !__result || !(t is Building))
                return;
            Building building = t as Building;
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
                            if (Utility.FindSpecificFuel(pawn, fuelUsed) == null)
                            {
                                JobFailReason.Is((string)"NoFuelToRefuel".Translate((NamedArgument)fuelUsed.label));
                                __result = false;
                            }
                        }
                        else if (!comp2.AllowedFuelFilter.Allows(fuelUsed))
                        {
                            JobFailReason.Is((string)"ESCP_Tools_FuelExtension_CannotRefuel_CurrentFuelDisallowed".Translate((NamedArgument)fuelUsed.label));
                            __result = false;
                        }
                    }
                    else
                    {
                        foreach (ThingDef allowedThingDef in comp2.AllowedFuelFilter.AllowedThingDefs)
                        {
                            if (Utility.FindSpecificFuel(pawn, allowedThingDef) != null)
                            {
                                __result = true;
                                return;
                            }
                        }
                        JobFailReason.Is((string)"ESCP_Tools_FuelExtension_CannotRefuel_AllFuelDisallowed".Translate((NamedArgument)t.Label));
                        __result = false;
                    }
                }
            }
        }
    }
}
