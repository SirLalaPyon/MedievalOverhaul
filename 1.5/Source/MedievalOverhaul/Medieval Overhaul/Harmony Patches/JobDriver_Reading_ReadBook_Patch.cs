using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(JobDriver_Reading), "ReadBook")]
    public static class JobDriver_Reading_ReadBook_Patch
    {
        public static void Postfix(Toil __result, JobDriver_Reading __instance)
        {
            if (Utility.VBEIsEnabled is false)
            {
                __result.AddFinishAction(delegate
                {
                    var pawn = __instance.pawn;
                    Room room = pawn.GetRoom();
                    if (room != null && room.Role == Utility.DankPyon_Library)
                    {
                        int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
                        if (pawn.needs.mood != null && Utility.DankPyon_ReadInLibrary.stages[scoreStageIndex] != null)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(Utility.DankPyon_ReadInLibrary, scoreStageIndex));
                        }
                    }
                });
            }
        }
    }
}
