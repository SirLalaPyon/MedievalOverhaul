using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{

	public class WorkGiver_RefuelStat : WorkGiver_Refuel
	{
		public override JobDef JobStandard => MedievalOverhaulDefOf.DankPyon_Slop_Refuel_Stat;

		public override JobDef JobAtomic => MedievalOverhaulDefOf.DankPyon_Slop_Refuel_StatAtomic;

		public override bool CanRefuelThing(Thing t)
		{
			return t is Building_SlopPot || t.TryGetComp<CompRefuelableStat>() != null;
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
			CompRefuelableStat comp1 = t.TryGetComp<CompRefuelableStat>();
			CompRefuelable comp2 = t.TryGetComp<CompRefuelable>();
            if (comp1 == null || comp1.IsFull || (!forced && !comp1.allowAutoRefuel))
            {
                return false;
            }
            if (comp1.FuelPercentOfMax > 0f && !comp1.Props.allowRefuelIfNotEmpty)
            {
                return false;
            }
            if (!forced && !comp1.ShouldAutoRefuelNow)
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
			if (comp2 != null)
			{
				if (!comp2.HasFuel)
				{
                    JobFailReason.Is(
                    "DankPyon_SlopFuel".Translate());
                    return false;
                }
			}
			if (Utility.FindBestFuel(pawn, t) == null)
			{
				JobFailReason.Is(
					"NoFuelToRefuel".Translate((NamedArgument)t.TryGetComp<CompRefuelableStat>().Props.fuelFilter
						.Summary));
				return false;
			}

			if (!t.TryGetComp<CompRefuelableStat>().Props.atomicFueling || Utility.FindAllFuel(pawn, t) != null)
				return true;
			JobFailReason.Is(
				"NoFuelToRefuel".Translate((NamedArgument)t.TryGetComp<CompRefuelableStat>().Props.fuelFilter.Summary));
			return false;
		}
		public static Job RefuelJob(
			Pawn pawn,
			Thing t,
			bool forced = false,
			JobDef customRefuelJob = null,
			JobDef customAtomicRefuelJob = null)
		{
			if (!t.TryGetComp<CompRefuelableStat>().Props.atomicFueling)
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