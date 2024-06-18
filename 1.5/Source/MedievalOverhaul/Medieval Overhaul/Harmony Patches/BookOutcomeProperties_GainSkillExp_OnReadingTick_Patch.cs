using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(BookOutcomeProperties_GainSkillExp), "OnReadingTick")]
    public static class BookOutcomeProperties_GainSkillExp_OnReadingTick_Patch
    {
        public static void Prefix(BookOutcomeProperties_GainSkillExp __instance, Pawn reader, ref float factor)
        {
            if (reader.jobs?.curDriver is JobDriver_Reading reading && reading.Book is BookWithAuthor withAuthor 
                && withAuthor.author == reader)
            {
                factor *= 0.5f;
            }
        }

    }
}
