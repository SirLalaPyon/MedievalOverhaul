using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{

	[HarmonyPatch(typeof(Building_NutrientPasteDispenser), nameof(Building_NutrientPasteDispenser.CanDispenseNow),
		MethodType.Getter)]
	public static class UnpoweredNutrientPaste
	{
		[HarmonyPrefix]
		public static bool CanDispenseNow(ref bool __result, Building_NutrientPasteDispenser __instance)
		{
			if (__instance.GetComp<CompUnpowered>() is null) return true;
			__result = __instance.HasEnoughFeedstockInHoppers() && (__instance.TryGetComp<CompRefuelable>()?.HasFuel ?? true);
			return false;
		}
	}

	[HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
	public static class ExpandDefinitionOfNutrientPasteDispenser
	{
		[HarmonyPrefix]
		public static bool Includes(this ThingRequestGroup group, ThingDef def, ref bool __result)
		{
			switch (group)
			{
				case ThingRequestGroup.FoodSource when def.IsFoodDispenser:
					__result = true;
					return false;
				case ThingRequestGroup.FoodSourceNotPlantOrTree when def.IsFoodDispenser:
					__result = true;
					return false;
				default:
					return true;
			}
		}
	}

	[HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PowerOn), MethodType.Getter)]
	public static class AlwaysOn
	{
		[HarmonyPostfix]
		public static bool PowerOn(bool result, CompPowerTrader __instance)
		{
			return result || (__instance is CompUnpowered && (__instance.parent.TryGetComp<CompRefuelable>()?.HasFuel ?? true));
		}
	}

	[HarmonyPatch(typeof(JobDriver_Ingest), nameof(JobDriver_Ingest.GetReport))]
	public static class ReportString
	{
		[HarmonyPrefix]
		public static bool GetReport(ref string __result, JobDriver_Ingest __instance, bool ___usingNutrientPasteDispenser, Job ___job, Pawn ___pawn)
		{
			if (!___usingNutrientPasteDispenser || ___job.targetA.Thing?.def is { } mealDef && mealDef == ThingDefOf.MealNutrientPaste) return true;
			ThingDef thingDef = ___job.targetA.Thing?.def;
			if (thingDef == null) return true;
			if (thingDef.ingestible != null)
			{
				if (!thingDef.ingestible.ingestReportStringEat.NullOrEmpty() &&
					(thingDef.ingestible.ingestReportString.NullOrEmpty() || ___pawn.RaceProps.intelligence < Intelligence.ToolUser))
					__result = thingDef.ingestible.ingestReportStringEat.Formatted((NamedArgument)___job.targetA.Thing.LabelShort, (NamedArgument)___job.targetA.Thing);
				if (!thingDef.ingestible.ingestReportString.NullOrEmpty())
					__result = thingDef.ingestible.ingestReportString.Formatted((NamedArgument)___job.targetA.Thing.LabelShort, (NamedArgument)___job.targetA.Thing);
			}
			else
			{
				__result = JobUtility.GetResolvedJobReportRaw(___job.def.reportString, thingDef.label, (object)thingDef, "", (object)"", "", (object)"");
			}

			return false;
		}
	}
}