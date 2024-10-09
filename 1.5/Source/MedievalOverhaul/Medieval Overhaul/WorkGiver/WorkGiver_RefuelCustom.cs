using System;
using System.Collections.Generic;
using System.Linq;
using ProcessorFramework;
using RimWorld;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{

	public class WorkGiver_RefuelCustom : WorkGiver_Scanner
    {
		public virtual JobDef JobStandard => MedievalOverhaulDefOf.DankPyon_RefuelCustom;

		public virtual JobDef JobAtomic => MedievalOverhaulDefOf.DankPyon_RefuelAtomicCustom;
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.GetComponent<RefuelableMapComponent>().refuelableCustomThing;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            RefuelableMapComponent mapComp = pawn.Map.GetComponent<RefuelableMapComponent>();
            return !mapComp.refuelableCustomThing.Any();
        }
        public bool CanRefuelThing(Thing t)
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
            if (FindBestFuel(pawn, t) == null)
            {
                ThingFilter fuelFilter = t.TryGetComp<CompRefuelableCustom>().Props.fuelFilter;
                JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter.Summary), null);
                return false;
            }
            if (t.TryGetComp<CompRefuelableCustom>().Props.atomicFueling && FindAllFuel(pawn, t) == null)
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
				Thing bestFuel = FindBestFuel(pawn, t);
				return JobMaker.MakeJob(customRefuelJob, (LocalTargetInfo)t, (LocalTargetInfo)bestFuel);
			}

			var allFuel = FindAllFuel(pawn, t);
			Job job = JobMaker.MakeJob(customAtomicRefuelJob, (LocalTargetInfo)t);
			job.targetQueueB = allFuel
				.Select(f => new LocalTargetInfo(f))
				.ToList();
			return job;
		}

        private static Thing FindBestFuel(Pawn pawn, Thing refuelable)
        {
            ThingFilter filter = refuelable.TryGetComp<CompRefuelableCustom>().AllowedFuelFilter;
            IEnumerable<Thing> searchSet = refuelable.Map.listerThings.ThingsMatchingFilter(filter);
            Predicate<Thing> validator = x =>
                !x.IsForbidden(pawn) && pawn.CanReserve((LocalTargetInfo)x) && filter.Allows(x);
            return GenClosest.ClosestThing_Global_Reachable_NewTemp(pawn.Position, pawn.Map, searchSet, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn), 9999f, validator);
        }

        private static List<Thing> FindAllFuel(Pawn pawn, Thing refuelable)
        {
            var countToFullyRefuel = refuelable.TryGetComp<CompRefuelableCustom>().GetFuelCountToFullyRefuel();
            ThingFilter filter = refuelable.TryGetComp<CompRefuelableCustom>().AllowedFuelFilter;
            return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, refuelable.Position,
                new IntRange(countToFullyRefuel, countToFullyRefuel), t => filter.Allows(t));
        }
    }
}