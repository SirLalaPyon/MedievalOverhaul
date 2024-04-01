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
        static readonly List<RecipeDef> mineRecipe = new List<RecipeDef>()
        {
            MedievalOverhaulDefOf.DankPyon_MakeStoneChunks_Coal,
            MedievalOverhaulDefOf.DankPyon_MakeStoneChunks_Sandstone,
            MedievalOverhaulDefOf.DankPyon_MakeStoneChunks_Granite,
            MedievalOverhaulDefOf.DankPyon_MakeStoneChunks_Limestone,
            MedievalOverhaulDefOf.DankPyon_MakeStoneChunks_Slate,
            MedievalOverhaulDefOf.DankPyon_MakeStoneChunks_Marble,
            MedievalOverhaulDefOf.DankPyon_MakeSalt,
            MedievalOverhaulDefOf.DankPyon_MakeSaltBulk,
            MedievalOverhaulDefOf.DankPyon_MakeTar,
            MedievalOverhaulDefOf.DankPyon_MakeTarBulk,
            MedievalOverhaulDefOf.DankPyon_MakeGunpowder,
            MedievalOverhaulDefOf.DankPyon_MakeGunpowderBulk,
            MedievalOverhaulDefOf.DankPyon_MakeOre_IronOre,
            MedievalOverhaulDefOf.DankPyon_MakeOre_IronOreBulk,
            MedievalOverhaulDefOf.DankPyon_MakeOre_SilverOre,
            MedievalOverhaulDefOf.DankPyon_MakeOre_SilverOreBulk,
        };

        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, RecipeDef recipeDef)
        {
            foreach (var thing in __result)
            {
                yield return thing;
            }
            if (mineRecipe.Contains(recipeDef) && __result != null)
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
