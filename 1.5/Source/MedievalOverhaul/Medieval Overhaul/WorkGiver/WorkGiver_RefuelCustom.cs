using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{

	public class WorkGiver_RefuelCustom : WorkGiver_Refuel
    {
		public override JobDef JobStandard => MedievalOverhaulDefOf.DankPyon_RefuelCustom;

		public override JobDef JobAtomic => MedievalOverhaulDefOf.DankPyon_RefuelAtomicCustom;

        public override bool CanRefuelThing(Thing t)
		{
			return t.TryGetComp<CompRefuelableCustom>() != null;
        }

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return CanRefuelThing(t) && CanRefuel(pawn, t, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return RefuelJob(pawn, t, forced, JobStandard, JobAtomic);
		}


		public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
		{
            CompRefuelableCustom compRefuelable = t.TryGetComp<CompRefuelableCustom>();
            if (compRefuelable == null || compRefuelable.IsFull || (!forced && !compRefuelable.allowAutoRefuel))
            {
                return false;
            }
            if (compRefuelable.FuelPercentOfMax > 0f && !compRefuelable.Props.allowRefuelIfNotEmpty)
            {
                return false;
            }
            if (!forced && !compRefuelable.ShouldAutoRefuelNow)
            {
                return false;
            }
            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            CompInteractable compInteractable = t.TryGetComp<CompInteractable>();
            if (compInteractable != null && compInteractable.Props.cooldownPreventsRefuel && compInteractable.OnCooldown)
            {
                JobFailReason.Is(compInteractable.Props.onCooldownString.CapitalizeFirst(), null);
                return false;
            }
            if (Utility.FindBestFuel(pawn, t) == null)
            {
                ThingFilter fuelFilter = t.TryGetComp<CompRefuelableCustom>().Props.fuelFilter;
                JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter.Summary), null);
                return false;
            }
            if (t.TryGetComp<CompRefuelableCustom>().Props.atomicFueling && Utility.FindAllFuel(pawn, t) == null)
            {
                ThingFilter fuelFilter2 = t.TryGetComp<CompRefuelableCustom>().Props.fuelFilter;
                JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter2.Summary), null);
                return false;
            }
            return true;
        }

        public static Job RefuelJob(
			Pawn pawn,
			Thing t,
			bool forced = false,
			JobDef customRefuelJob = null,
			JobDef customAtomicRefuelJob = null)
		{
			if (!t.TryGetComp<CompRefuelableCustom>().Props.atomicFueling)
			{
				Thing bestFuel = Utility.FindBestFuel(pawn, t);
				return JobMaker.MakeJob(customRefuelJob ?? JobDefOf.Refuel, (LocalTargetInfo)t, (LocalTargetInfo)bestFuel);
			}

			var allFuel = Utility.FindAllFuel(pawn, t);
			Job job = JobMaker.MakeJob(customAtomicRefuelJob ?? JobDefOf.RefuelAtomic, (LocalTargetInfo)t);
			job.targetQueueB = allFuel
				.Select(f => new LocalTargetInfo(f))
				.ToList();
			return job;
		}
	}
}