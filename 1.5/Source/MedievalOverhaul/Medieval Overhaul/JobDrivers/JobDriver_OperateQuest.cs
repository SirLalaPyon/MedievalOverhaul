using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace MedievalOverhaul
{
    public class JobDriver_OperateQuest: JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, errorOnFailed, false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompQuestFinder scannerComp = this.job.targetA.Thing.TryGetComp<CompQuestFinder>();
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => !scannerComp.CanUseNow);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell, false);
            Toil work = ToilMaker.MakeToil("MakeNewToils");
            work.tickAction = delegate ()
            {
                Pawn actor = work.actor;
                Building_QuestScanner building = (Building_QuestScanner)actor.CurJob.targetA.Thing;
                building.UsedThisTick();
                scannerComp.Used(actor);
                actor.skills.Learn(SkillDefOf.Intellectual, 0.035f, false, false);
                actor.GainComfortFromCellIfPossible(true);
            };
            work.PlaySustainerOrSound(scannerComp.Props.soundWorking, 1f);
            work.AddFailCondition(() => !scannerComp.CanUseNow);
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            work.activeSkill = (() => SkillDefOf.Intellectual);
            yield return work;
            yield break;
        }
    }
}
