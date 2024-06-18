using HarmonyLib;
using RimWorld;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(RoomRoleWorker_Workshop), "GetScore")]
    public static class RoomRoleWorker_Workshop_GetScore_Patch
    {
        public static void Prefix()
        {
            MedievalOverhaulDefOf.DankPyon_Book_ScribeTable.designationCategory = null;
        }

        public static void Postfix()
        {
            MedievalOverhaulDefOf.DankPyon_Book_ScribeTable.designationCategory = DesignationCategoryDefOf.Production;
        }
    }
}
