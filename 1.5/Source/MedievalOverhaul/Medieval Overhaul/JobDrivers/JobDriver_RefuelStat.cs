using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{
	public class JobDriver_RefuelStat : JobDriver
	{
		private const TargetIndex RefuelableInd = TargetIndex.A;

		private const TargetIndex FuelInd = TargetIndex.B;

		private const int RefuelingDuration = 240;

		protected Thing Refuelable => job.GetTarget(RefuelableInd).Thing;

		protected CompRefuelableStat RefuelableComp => Refuelable.TryGetComp<CompRefuelableStat>();

		protected Thing Fuel => job.GetTarget(FuelInd).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed)
				   && pawn.Reserve(Fuel, job, 1, -1, null, errorOnFailed);
		}

        public override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(RefuelableInd);
			AddEndCondition(() => !RefuelableComp.IsFull ? JobCondition.Ongoing : JobCondition.Succeeded);
			AddFailCondition(() => !job.playerForced && !RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct);
			AddFailCondition(() => !RefuelableComp.allowAutoRefuel && !job.playerForced);
			yield return Toils_General.DoAtomic(delegate { job.count = RefuelableComp.GetFuelCountToFullyRefuel(Fuel); });
			Toil reserveFuel = Toils_Reserve.Reserve(FuelInd);
			yield return reserveFuel;
			yield return Toils_Goto.GotoThing(FuelInd, PathEndMode.ClosestTouch)
				.FailOnDespawnedNullOrForbidden(FuelInd).FailOnSomeonePhysicallyInteracting(FuelInd);
			yield return Toils_Haul
				.StartCarryThing(FuelInd, false, true)
				.FailOnDestroyedNullOrForbidden(FuelInd);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, FuelInd, TargetIndex.None,
				true);
			yield return Toils_Goto.GotoThing(RefuelableInd, PathEndMode.InteractionCell);
			yield return Toils_General.Wait(RefuelingDuration).FailOnDestroyedNullOrForbidden(FuelInd)
				.FailOnDestroyedNullOrForbidden(RefuelableInd)
				.FailOnCannotTouch(RefuelableInd, PathEndMode.Touch)
				.WithProgressBarToilDelay(RefuelableInd);
			yield return FinalizeRefueling(RefuelableInd, FuelInd);
		}

		public static Toil FinalizeRefueling(TargetIndex refuelableInd, TargetIndex fuelInd)
		{
			Toil toil = ToilMaker.MakeToil();
			toil.initAction = () =>
			{
				Job curJob = toil.actor.CurJob;
				Thing thing = curJob.GetTarget(refuelableInd).Thing;
				if (toil.actor.CurJob.placedThings.NullOrEmpty())
                    thing.TryGetComp<CompRefuelableStat>().Refuel(
                    [
                    curJob.GetTarget(fuelInd).Thing
					]);
				else
					thing.TryGetComp<CompRefuelableStat>().Refuel(toil.actor.CurJob.placedThings
						.Select(p => p.thing).ToList());
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
	}
}