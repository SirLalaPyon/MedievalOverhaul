using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class MakeRecipeProducts_Postfix
    {
        static readonly List<ThingDef> bonusGems = new List<ThingDef>()
        {
            MedievalOverhaulDefOf.DankPyon_Citrine,
            MedievalOverhaulDefOf.DankPyon_Amber,
            MedievalOverhaulDefOf.DankPyon_Onyx,
            MedievalOverhaulDefOf.DankPyon_Emerald,
            MedievalOverhaulDefOf.DankPyon_Sapphire,
            MedievalOverhaulDefOf.DankPyon_Ruby,
            MedievalOverhaulDefOf.DankPyon_GoldOre,
        };
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, List<Thing> ingredients)
        {
            if (__result != null)
            {
                if (recipeDef.HasModExtension<RecipeExtension_Timber>())
                {
                    int num = 0;
                    Thing thing = null;
                    thing = ingredients[0];
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        num += thing.stackCount * 2;
                    }
                    ThingDefCountClass thingDefCountClass = thing.def.butcherProducts[0];
                    ThingDef butcherDef = thingDefCountClass.thingDef;
                    Thing butcherThing = ThingMaker.MakeThing(butcherDef, null);
                    butcherThing.stackCount = num;
                    yield return butcherThing;
                }
                else foreach (var thing in __result)
                {
                    yield return thing;
                }
                if (recipeDef.HasModExtension<RecipeExtension_Mine>())
                {
                    double recipeWorkAmount = recipeDef.workAmount / 600f;
                    double roundedNumber = Math.Round(recipeWorkAmount) * 0.01;
                    float randChance = ((float)roundedNumber + 0.01f);
                    if (Rand.Chance(randChance))
                    {
                        int randomInRange = Rand.RangeInclusive(0, bonusGems.Count - 1);
                        ThingDef bonusGemDef = bonusGems[randomInRange];
                        Thing bonusGem = ThingMaker.MakeThing(bonusGemDef);
                        bonusGem.stackCount = 1;
                        yield return bonusGem;
                    }
                }
            }
        }
    }
}
