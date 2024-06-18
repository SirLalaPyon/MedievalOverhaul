using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{

    [HarmonyPatch(typeof(BookOutcomeDoerGainSkillExp), "OnReadingTick")]
    public static class BookOutcomeDoerGainSkillExp_OnReadingTick_Patch
    {
        public static void Prefix(Pawn reader, ref float factor)
        {
            if (reader.jobs?.curDriver is JobDriver_Reading reading && reading.Book is BookWithAuthor withAuthor 
                && withAuthor.author == reader)
            {
                factor *= 0.5f;
            }
        }

    }
}
