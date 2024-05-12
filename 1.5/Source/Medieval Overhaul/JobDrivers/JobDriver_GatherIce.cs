using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using System.Security.Cryptography;

namespace MedievalOverhaul
{
    public class JobDriver_GatherIce : JobDriver_AffectFloor
    {

        protected override int BaseWorkAmount => 600;

        protected override DesignationDef DesDef => MedievalOverhaulDefOf.DankPyon_GatherIce;

        protected override StatDef SpeedStat => StatDefOf.MiningSpeed;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, ReservationLayerDefOf.Floor, errorOnFailed, false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => (!job.ignoreDesignations && base.Map.designationManager.DesignationAt(base.TargetLocA, DesDef) == null) ? true : false);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = new Toil();
            doWork.initAction = delegate
            {
                workLeft = BaseWorkAmount;
            };
            doWork.tickAction = delegate
            {
                float num = ((SpeedStat == null) ? 1f : StatExtension.GetStatValue((Thing)doWork.actor, SpeedStat, true));
                workLeft -= num;
                if (doWork.actor.skills != null)
                {
                    doWork.actor.skills.Learn(SkillDefOf.Mining, 0.03f);
                }
                base.Map.snowGrid.SetDepth(base.TargetLocA, 0f);
                if (workLeft <= 0f)
                {
                    DoEffect(base.TargetLocA);
                    base.Map.designationManager.DesignationAt(base.TargetLocA, DesDef)?.Delete();
                    ReadyForNextToil();
                }
            };
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            doWork.WithProgressBar(TargetIndex.A, () => 1f - workLeft / (float)BaseWorkAmount);
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.activeSkill = () => SkillDefOf.Mining;
            yield return doWork;
        }

        protected override void DoEffect(IntVec3 c)
        {
            TerrainDef terrain = base.Map.terrainGrid.TerrainAt(c);
            if (terrain == TerrainDefOf.Ice)
            {
                Thing thing = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_IceBlock);
                thing.stackCount = 5;
                GenPlace.TryPlaceThing(thing, base.TargetLocA, base.Map, ThingPlaceMode.Near);
            }
            if (terrain.IsWater)
            {
                Thing thing = ThingMaker.MakeThing(MedievalOverhaulDefOf.DankPyon_Waterskin);
                thing.stackCount = 1;
                GenPlace.TryPlaceThing(thing, base.TargetLocA, base.Map, ThingPlaceMode.Near);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
        }

        private float workLeft = -1000f;

        protected new bool clearSnow;
    }
}