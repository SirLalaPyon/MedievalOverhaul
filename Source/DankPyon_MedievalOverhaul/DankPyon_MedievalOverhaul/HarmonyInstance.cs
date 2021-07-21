using HarmonyLib;
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
            if (__instance is Pawn pawn && pawn.RaceProps.IsFlesh)
            {
                int amount = Math.Max(1, (int)(pawn.BodySize * 0.2f));
                Thing Bone = ThingMaker.MakeThing(bone, null);
                Bone.stackCount = amount;
                __result = __result.AddItem(Bone);
                Thing Fat = ThingMaker.MakeThing(fat, null);
                Fat.stackCount = amount;
                __result = __result.AddItem(Fat);
            }
        }
    }
}
