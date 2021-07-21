using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DankPyon_MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class HarmonyInstance
    {
        static HarmonyInstance()
        {
            new Harmony("lalapyhon.rimworld.medievaloverhaul").Patch(AccessTools.Method(typeof(Thing), "ButcherProducts", null, null), null, new HarmonyMethod(typeof(HarmonyInstance), "Thing_MakeButcherProducts_FatAndBone_PostFix", null), null);
        }


        private static ThingDef fat = ThingDef.Named("DankPyon_Fat");
        private static ThingDef bone = ThingDef.Named("DankPyon_Bone");
        private static void Thing_MakeButcherProducts_FatAndBone_PostFix(Thing __instance, ref IEnumerable<Thing> __result, Pawn butcher, float efficiency)
        {
            if (__instance is Pawn pawn && pawn.RaceProps.IsFlesh && pawn.RaceProps.meatDef != null)
            {
                Thing Bone = ThingMaker.MakeThing(bone, null);
                Thing Fat = ThingMaker.MakeThing(fat, null);
                int amount = Math.Max(1, (int)(GenMath.RoundRandom(__instance.GetStatValue(StatDefOf.MeatAmount, true) * efficiency) * 0.2f));
                Bone.stackCount = amount;
                Fat.stackCount = amount;
                __result = __result.AddItem(Bone);
                __result = __result.AddItem(Fat);
            }
        }
    }
}
