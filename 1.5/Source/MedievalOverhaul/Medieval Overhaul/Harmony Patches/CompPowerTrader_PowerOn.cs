using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PowerOn), MethodType.Getter)]
    public static class CompPowerTrader_PowerOn
    {
        [HarmonyPostfix]
        public static bool Postfix(bool result, CompPowerTrader __instance)
        {
            return result || (__instance is CompUnpowered && (__instance.parent.TryGetComp<CompRefuelable>()?.HasFuel ?? true));
        }
    }
}
