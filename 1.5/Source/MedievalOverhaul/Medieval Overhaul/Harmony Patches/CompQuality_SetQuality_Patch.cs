using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(CompQuality), "SetQuality")]
    public static class CompQuality_SetQuality_Patch
    {
        public static void Prefix(CompQuality __instance, ref QualityCategory q)
        {
            if (__instance.parent is Book book && book.BookComp.Props is CompProperties_DefinableBook definableBook)
            {
                if (definableBook.qualityRange.HasValue)
                {
                    if ((int)q < definableBook.qualityRange.Value.min)
                    {
                        q = (QualityCategory)definableBook.qualityRange.Value.min;
                    }
                    else if ((int)q > definableBook.qualityRange.Value.max)
                    {
                        q = (QualityCategory)definableBook.qualityRange.Value.max;
                    }
                }
            }
        }
    }
}
