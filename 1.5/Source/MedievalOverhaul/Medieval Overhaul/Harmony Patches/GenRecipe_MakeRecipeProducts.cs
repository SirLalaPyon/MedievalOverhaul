using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class GenRecipe_MakeRecipeProducts
    {
        public static Pawn curWorker;
        public static RecipeDef curRecipe;
        [HarmonyPriority(int.MaxValue)]
        public static void Prefix(RecipeDef recipeDef, Pawn worker)
        {
            if (recipeDef.HasModExtension<TreatiseSkill>())
            {
                curWorker = worker;
                curRecipe = recipeDef;
            }
        }
        [HarmonyPriority(int.MinValue)]
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef, List<Thing> ingredients)
        {
            if (__result != null)
            {
                foreach (var thing in __result)
                {
                    yield return thing;
                }
                if (recipeDef.HasModExtension<RecipeExtension_Timber>())
                {
                    Thing thing = null;
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        thing = ingredients[i];
                        if (thing?.def?.butcherProducts?.Count <= 0)
                        {
                            Log.Error("Attempting to butcher object without butcherProducts. Please report this to the authers of " + ingredients[i].ContentSource.Name + ". " + "RecipeDef: " + recipeDef + " " +  " Ingredient: " + ingredients[i].def);
                            break;
                        }
                        for (int j = 0; j < thing.def.butcherProducts.Count; j++)
                        {
                            ThingDefCountClass thingDefCountClass = thing.def.butcherProducts[j];
                            int num = thing.stackCount * thingDefCountClass.count;
                            if (num > 2)
                            {
                                num -= 2;
                            }
                            ThingDef butcherDef = thingDefCountClass.thingDef;
                            Thing butcherThing = ThingMaker.MakeThing(butcherDef, null);
                            butcherThing.stackCount = num;
                            yield return butcherThing;
                        }
                    }
                    yield break;
                }
                var mineExtension = recipeDef.GetModExtension<RecipeExtension_Mine>();
                if (mineExtension != null)
                {
                    List<ThingDef> bonusGems = mineExtension.bonusGems;
                    int bonusGemCount = bonusGems.Count;
                    if (bonusGemCount > 0)
                    {
                        double recipeWorkAmount = recipeDef.workAmount / mineExtension.workAmountPerChance;
                        double roundedNumber = Math.Round(recipeWorkAmount) * mineExtension.randomChance;
                        float randChance = ((float)roundedNumber + 0.01f);
                        if (Rand.Chance(randChance))
                        {
                            int randomInRange = Rand.Range(0, bonusGemCount);
                            ThingDef bonusGemDef = bonusGems[randomInRange];
                            Thing gem = ThingMaker.MakeThing(bonusGemDef);
                            gem.stackCount = 1;
                            yield return gem;
                        }
                        yield break;
                    }
                    
                }
            }

            curWorker = null;
            curRecipe = null;
        }
    }
}
