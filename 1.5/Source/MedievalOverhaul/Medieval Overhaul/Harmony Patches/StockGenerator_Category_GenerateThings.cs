using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(StockGenerator_Category), "GenerateThings")]
    public static class StockGenerator_Category_GenerateThings
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result)
        {
            if (__result != null)
            {
                foreach (var thing in __result)
                {
                    if (HideUtility.IsHide(thing.def))
                    {
                        var comp = thing.TryGetComp<CompGenericHide>();
                        if (comp != null)
                        {
                            int num1 = thing.stackCount;
                            int parts = num1 / 50;
                            int remainder = num1 % 50;
                            if (parts > 0)
                            {
                                thing.stackCount = parts;
                                comp.leatherAmount = 50 + remainder / parts;
                            }
                            else
                            {
                                thing.stackCount = 1;
                                comp.leatherAmount = num1;

                            }
                            var leatherCost = comp.Props.leatherType.GetStatValueAbstract(StatDefOf.MarketValue);
                            comp.marketValue = (int)((int)(comp.leatherAmount * leatherCost) * 0.8f);
                            comp.massValue = (comp.leatherAmount * comp.Props.leatherType.GetStatValueAbstract(StatDefOf.Mass));
                        }
                    }
                    yield return thing;
                }
            }
        }
    }
}
