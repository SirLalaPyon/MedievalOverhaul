using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(BookOutcomeDoerGainSkillExp), "OnBookGenerated")]
    public static class BookOutcomeDoerGainSkillExp_OnBookGenerated_Patch
    {
        public static bool Prefix(BookOutcomeDoerGainSkillExp __instance)
        {
            if (GenRecipe_MakeRecipeProducts_Patch.curWorker != null)
            {
                var extension = GenRecipe_MakeRecipeProducts_Patch.curRecipe.GetModExtension<TreatiseSkill>();
                float num = BookUtility.GetSkillExpForQuality(__instance.Quality);
                __instance.values[extension.skill] = num;
                return false;
            }
            return true;
        }
    }
}
