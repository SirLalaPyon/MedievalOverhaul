using HarmonyLib;
using RimWorld;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(Bill), "Notify_PawnDidWork")]
    public static class Bill_Notify_PawnDidWork_Patch
    {
        public static void Postfix(Bill __instance)
        {
            __instance.TryRefreshLit();
        }

        public static void TryRefreshLit(this Bill __instance)
        {
            if (__instance.billStack.billGiver is Building_WorkTable thing)
            {
                var comp = thing.GetComp<CompGlowerOnlyWhenUsed>();
                comp?.UpdateLit(thing.Map);
            }
        }
    }
}
