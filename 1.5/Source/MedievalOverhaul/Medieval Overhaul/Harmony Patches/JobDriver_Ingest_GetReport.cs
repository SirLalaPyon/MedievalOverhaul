using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace MedievalOverhaul.Patches
{
    [HarmonyPatch(typeof(JobDriver_Ingest), nameof(JobDriver_Ingest.GetReport))]
    public static class JobDriver_Ingest_GetReport
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string __result, bool ___usingNutrientPasteDispenser, Job ___job, Pawn ___pawn)
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
