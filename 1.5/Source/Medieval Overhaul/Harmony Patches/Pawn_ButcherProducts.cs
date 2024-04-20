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
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ButcherProducts))]
    public static class Pawn_ButcherProducts
    {
        private static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn __instance, Pawn butcher, float efficiency)
        {
            List<Thing> productList = new List<Thing>();
            // Adding normal butcher output to the list
            foreach (Thing product in __result)
            {
                productList.Add(product);
            }
            // Checking for additional butcher products and adding to list
            var additionalButcherOptions = __instance.def.GetModExtension<AdditionalButcherProducts>();
            if (additionalButcherOptions != null)
            {
                foreach (var option in additionalButcherOptions.butcherOptions)
                {
                    if (Rand.Chance(option.chance))
                    {
                        Thing product = ThingMaker.MakeThing(option.thingDef, null);
                        product.stackCount = option.amount.RandomInRange;
                        productList.Add(product);
                    }
                }
            }
            // Checking to add Bone and Fat and adding to list
            if (__instance.RaceProps.IsFlesh && __instance.RaceProps.meatDef != null && !__instance.RaceProps.Insect)
            {
                bool boneFlag = true;
                bool fatFlag = true;
                var butcherProperties = ButcherProperties.Get(__instance.def);
                if (butcherProperties != null)
                {
                    boneFlag = butcherProperties.hasBone;
                    fatFlag = butcherProperties.hasFat;
                }
                if (boneFlag || fatFlag)
                {
                    int amount = Math.Max(1, (int)(GenMath.RoundRandom(__instance.GetStatValue(StatDefOf.MeatAmount, true) * efficiency) * 0.1f));
                    if (boneFlag)
                    {
                        Thing bone = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_Bone, null);
                        bone.stackCount = amount;

                        productList.Add(bone);
                    }
                    if (fatFlag)
                    {
                        Thing fat = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_Fat, null);
                        fat.stackCount = amount;
                        productList.Add(fat);
                    }
                }
            }
            // Taking list and pushing it out
            IEnumerable<Thing> output = productList;
            __result = output;
            return __result;
        }
    }
}
