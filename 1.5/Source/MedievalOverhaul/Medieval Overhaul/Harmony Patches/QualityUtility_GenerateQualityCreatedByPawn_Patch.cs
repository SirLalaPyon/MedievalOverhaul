using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(QualityUtility))]
    [HarmonyPatch("GenerateQualityCreatedByPawn")]
    [HarmonyPatch(new Type[]
    {
            typeof(Pawn),
            typeof(SkillDef)
    }, new ArgumentType[]
    {
            0,
            0
    })]
    public static class QualityUtility_GenerateQualityCreatedByPawn_Patch
    {
        public static List<(int, QualityCategory)> minQualities = new List<(int, QualityCategory)>
        {
            (9, QualityCategory.Poor),
            (13, QualityCategory.Normal),
            (17, QualityCategory.Good),
            (20, QualityCategory.Excellent),
        };

        private static void Postfix(ref QualityCategory __result, Pawn pawn)
        {
            if (pawn.CurJob?.RecipeDef.WorkerCounter is RecipeWorker_MakeSkillBook)
            {
                var intellectual = pawn.skills.GetSkill(SkillDefOf.Intellectual);
                var minQuality = QualityCategory.Awful;
                foreach (var min in minQualities)
                {
                    if (intellectual.Level >= min.Item1)
                    {
                        minQuality = min.Item2;
                    }
                }
                if (__result < minQuality)
                {
                    __result = minQuality;
                }
            }
        }
    }
}
