using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "EndCurrentJob")]
    public static class Pawn_JobTracker_EndCurrentJob_Patch
    {
        public static void Prefix(Pawn_JobTracker __instance, out Thing __state)
        {
            __state = __instance.curJob?.targetA.Thing;
        }
        public static void Postfix(Thing __state)
        {
            if (__state != null)
            {
                var comp = __state.TryGetComp<CompGlowerOnlyWhenUsed>();
                comp?.UpdateLit(__state.Map);
            }
        }
    }
}
