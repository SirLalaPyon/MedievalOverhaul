using HarmonyLib;
using ProcessorFramework;

namespace MedievalOverhaul
{
    [HarmonyPatch(typeof(ActiveProcess), "DoTicks")]
    public static class ActiveProcess_DoTicks_Patch
    {
        public static void Postfix(ActiveProcess __instance, CompProcessor ___processor)
        {
            var thing = ___processor.parent;
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
