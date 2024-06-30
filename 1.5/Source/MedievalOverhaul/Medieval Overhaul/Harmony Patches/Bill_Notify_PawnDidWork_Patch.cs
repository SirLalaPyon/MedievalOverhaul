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
            var thing = __instance.billStack.billGiver as Building_WorkTable;
            if (thing != null)
            {
                var comp = thing.GetComp<CompGlowerOnlyWhenUsed>();
                if (comp != null)
                {
                    comp.UpdateLit(thing.Map);
                }
            }
        }
    }
}
