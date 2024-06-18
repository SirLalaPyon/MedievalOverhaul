using HarmonyLib;
using MedievalOverhaul.Patches;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(BookOutcomeDoerGainSkillExp), "OnBookGenerated")]
    public static class BookOutcomeDoerGainSkillExp_OnBookGenerated_Patch
    {
        public static bool Prefix(BookOutcomeDoerGainSkillExp __instance)
        {
            float num = BookUtility.GetSkillExpForQuality(__instance.Quality);
            var overrides = new Dictionary<SkillDef, float>();
            if (__instance is BookOutcomeProperties_GainSkillExpDefinable compProps)
            {
                if (compProps.skillGains != null)
                {
                    foreach (var skill in compProps.skillGains)
                    {
                        overrides[skill.skillDef] = num * skill.gainMultiplier;
                    }
                }
            }

            if (GenRecipe_MakeRecipeProducts.curWorker != null)
            {
                var extension = GenRecipe_MakeRecipeProducts.curRecipe.GetModExtension<TreatiseSkill>();
                if (extension != null)
                {
                    overrides[extension.skill] = num;
                }
            }
            if (overrides.Count > 0)
            {
                __instance.values = overrides;
                return false;
            }
            return true;
        }
    }
}
