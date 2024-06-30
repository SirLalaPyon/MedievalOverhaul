using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(BookOutcomeDoerGainSkillExp), "OnReadingTick")]
    public static class BookOutcomeDoerGainSkillExp_OnReadingTick_Patch
    {
        public static void Prefix(BookOutcomeDoerGainSkillExp __instance, Pawn reader, ref float factor)
        {
            BookOutcomeDoerGainSkillExp_GetMaxSkillLevel_Patch.curDoer = __instance;
            if (reader.jobs?.curDriver is JobDriver_Reading reading && reading.Book is BookWithAuthor withAuthor 
                && withAuthor.author == reader)
            {
                factor *= 0.5f;
            }
        }

        public static void Postfix()
        {
            BookOutcomeDoerGainSkillExp_GetMaxSkillLevel_Patch.curDoer = null;
        }
    }

    [HarmonyPatch(typeof(BookOutcomeDoerGainSkillExp), "GetBenefitsString")]
    public static class BookOutcomeDoerGainSkillExp_GetBenefitsString_Patch
    {
        public static void Prefix(BookOutcomeDoerGainSkillExp __instance)
        {
            BookOutcomeDoerGainSkillExp_GetMaxSkillLevel_Patch.curDoer = __instance;
        }
        public static void Postfix()
        {
            BookOutcomeDoerGainSkillExp_GetMaxSkillLevel_Patch.curDoer = null;
        }
    }

    [HarmonyPatch(typeof(BookOutcomeDoerGainSkillExp), "DoesProvidesOutcome")]
    public static class BookOutcomeDoerGainSkillExp_DoesProvidesOutcome_Patch
    {
        public static void Prefix(BookOutcomeDoerGainSkillExp __instance)
        {
            BookOutcomeDoerGainSkillExp_GetMaxSkillLevel_Patch.curDoer = __instance;
        }

        public static void Postfix()
        {
            BookOutcomeDoerGainSkillExp_GetMaxSkillLevel_Patch.curDoer = null;
        }
    }

    [HarmonyPatch(typeof(BookOutcomeDoerGainSkillExp), "GetMaxSkillLevel")]
    public static class BookOutcomeDoerGainSkillExp_GetMaxSkillLevel_Patch
    {
        public static BookOutcomeDoerGainSkillExp curDoer;
        public static void Postfix(BookOutcomeDoerGainSkillExp __instance, ref int __result)
        {
            if (curDoer is not null && __instance.Props is BookOutcomeProperties_GainSkillExpDefinable compProps &&
                compProps.maxSkillLevel.HasValue)
            {
                __result = compProps.maxSkillLevel.Value;
            }
        }
    }
}
