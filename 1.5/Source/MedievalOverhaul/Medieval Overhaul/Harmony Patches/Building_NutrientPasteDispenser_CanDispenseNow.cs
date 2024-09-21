using HarmonyLib;
using RimWorld;
using Verse;

namespace MedievalOverhaul.Patches
{

	[HarmonyPatch(typeof(Building_NutrientPasteDispenser), nameof(Building_NutrientPasteDispenser.CanDispenseNow),
		MethodType.Getter)]
	public static class Building_NutrientPasteDispenser_CanDispenseNow
	{
		[HarmonyPrefix]
		public static bool Prefix(ref bool __result, Building_NutrientPasteDispenser __instance)
		{
			if (__instance.GetComp<CompUnpowered>() is null) return true;
			__result = __instance.HasEnoughFeedstockInHoppers() && (__instance.TryGetComp<CompRefuelable>()?.HasFuel ?? true);
			return false;
		}
	}

}