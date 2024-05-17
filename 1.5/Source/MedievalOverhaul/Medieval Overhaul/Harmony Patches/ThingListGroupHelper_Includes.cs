using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
    public static class ThingListGroupHelper_Includes
    {
        [HarmonyPrefix]
        public static bool Prefix(this ThingRequestGroup group, ThingDef def, ref bool __result)
        {
            switch (group)
            {
                case ThingRequestGroup.FoodSource when def.IsFoodDispenser:
                    __result = true;
                    return false;
                case ThingRequestGroup.FoodSourceNotPlantOrTree when def.IsFoodDispenser:
                    __result = true;
                    return false;
                default:
                    return true;
            }
        }
    }
}
