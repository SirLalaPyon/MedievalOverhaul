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
    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class StatExtension_GetStatValue
    {
        private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
        {

            if (HideUtility.IsHide(thing.def))
            {
                if (stat == StatDefOf.MarketValue)
                {
                    var comp = thing.TryGetComp<CompGenericHide>();
                    if (comp != null)
                    {
                        if (comp.leatherAmount > comp.Props.leatherAmount)
                        __result = comp.marketValue;
                    }
                }
                if (stat == StatDefOf.Mass)
                {
                    var comp = thing.TryGetComp<CompGenericHide>();
                    if (comp != null)
                    {
                        if (comp.leatherAmount > comp.Props.leatherAmount)
                        __result = comp.massValue;
                    }
                }
            }
        }
    }
}
