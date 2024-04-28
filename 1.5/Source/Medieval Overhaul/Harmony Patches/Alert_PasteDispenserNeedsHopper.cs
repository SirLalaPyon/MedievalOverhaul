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

    [HarmonyPatch(typeof(Alert_PasteDispenserNeedsHopper), "BadDispensers", MethodType.Getter)]
    public static class Alert_BadDispensers
    {
        static List <Thing> Postfix(List<Thing> __result)
        {
            for (int i = 0; i < __result.Count; i++)
            {
                Building_SlopPot slopPot = __result[i] as Building_SlopPot;
                if (slopPot != null)
                {
                    __result.RemoveAt(i);
                    i--;
                }
            }
            return __result;
        }
    }
}
