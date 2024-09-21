using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(Alert_PasteDispenserNeedsHopper), "BadDispensers", MethodType.Getter)]
    public static class Alert_BadDispensers
    {
        public static List <Thing> Postfix(List<Thing> __result)
        {
            for (int i = 0; i < __result.Count; i++)
            {
                if (__result[i] is Building_SlopPot)
                {
                    __result.RemoveAt(i);
                    i--;
                }
            }
            return __result;
        }
    }
}
