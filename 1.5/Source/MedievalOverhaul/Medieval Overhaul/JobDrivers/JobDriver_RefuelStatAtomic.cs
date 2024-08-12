using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MedievalOverhaul
{
	public class JobDriver_RefuelStatAtomic : JobDriver_RefuelStat
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			return pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed);
		}

        protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			AddEndCondition(() => !RefuelableComp.IsFull ? JobCondition.Ongoing : JobCondition.Succeeded);
			AddFailCondition(() =>
				(!job.playerForced && !RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct) || !RefuelableComp.allowAutoRefuel);
			AddFailCondition(() => !RefuelableComp.allowAutoRefuel && !job.playerForced);
			yield return Toils_General.DoAtomic(delegate { job.count = RefuelableComp.GetFuelCountToFullyRefuel(Fuel); });
			Toil getNextIngredient = Toils_General.Label();
			yield return getNextIngredient;
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
				.FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul
				.StartCarryThing(TargetIndex.B, false, true)
				.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil findPlaceTarget =
				Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
			yield return findPlaceTarget;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, false);
			yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
			yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.A)
				.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
				.WithProgressBarToilDelay(TargetIndex.A);
			yield return FinalizeRefueling(TargetIndex.A, TargetIndex.None);
		}
	}
}