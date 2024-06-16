using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace MedievalOverhaul
{
    public class JobDriver_Mine_Golem : JobDriver
    {
        private Thing MineTarget
        {
            get
            {
                return this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.MineTarget, this.job, 1, -1, null, errorOnFailed, false);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => this.pawn.IsPlayerControlled && !this.job.ignoreDesignations && this.pawn.Map.designationManager.DesignationAt(this.TargetA.Cell, DesignationDefOf.Mine) == null && this.pawn.Map.designationManager.DesignationAt(this.TargetA.Cell, DesignationDefOf.MineVein) == null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, false);
            Toil mine = ToilMaker.MakeToil("MakeNewToils");
            mine.tickAction = delegate ()
            {
                Pawn actor = mine.actor;
                Thing mineTarget = this.MineTarget;
                if (this.ticksToPickHit < -100)
                {
                    this.ResetTicksToPickHit();
                }
                if (actor.skills != null && (mineTarget.Faction != actor.Faction || actor.Faction == null))
                {
                    actor.skills.Learn(SkillDefOf.Mining, 0.07f, false, false);
                }
                this.ticksToPickHit--;
                if (this.ticksToPickHit <= 0)
                {
                    IntVec3 position = mineTarget.Position;
                    if (this.effecter == null)
                    {
                        this.effecter = EffecterDefOf.Mine.Spawn();
                    }
                    this.effecter.Trigger(actor, mineTarget, -1);
                    int num = mineTarget.def.building.isNaturalRock ? 80 : 40;
                    Mineable_CompSpawnerDestroy mineable = mineTarget as Mineable_CompSpawnerDestroy;
                    if (mineable == null || mineTarget.HitPoints > num)
                    {
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Mining, (float)num, 0f, -1f, mine.actor, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true);
                        mineTarget.TakeDamage(dinfo);
                    }
                    else
                    {
                        bool flag = this.Map.designationManager.DesignationAt(mineable.Position, DesignationDefOf.MineVein) != null;
                        mineable.Notify_TookMiningDamage(mineTarget.HitPoints, mine.actor);
                        mineable.HitPoints = 0;
                        mineable.DestroyMined(actor);
                        if (flag)
                        {
                            foreach (IntVec3 b in GenAdj.AdjacentCells)
                            {
                                Designator_MineVein.FloodFillDesignations(position + b, this.Map, mineable.def);
                            }
                        }
                    }
                    if (mineTarget.Destroyed)
                    {
                        actor.Map.mineStrikeManager.CheckStruckOre(position, mineTarget.def, actor);
                        actor.records.Increment(RecordDefOf.CellsMined);
                        if (this.pawn.Faction != Faction.OfPlayer)
                        {
                            List<Thing> thingList = position.GetThingList(this.Map);
                            for (int j = 0; j < thingList.Count; j++)
                            {
                                thingList[j].SetForbidden(true, false);
                            }
                        }
                        if (this.pawn.Faction == Faction.OfPlayer && MineStrikeManager.MineableIsVeryValuable(mineTarget.def))
                        {
                            TaleRecorder.RecordTale(TaleDefOf.MinedValuable, new object[]
                            {
                                this.pawn,
                                mineTarget.def.building.mineableThing
                            });
                        }
                        if (this.pawn.Faction == Faction.OfPlayer && MineStrikeManager.MineableIsValuable(mineTarget.def) && !this.pawn.Map.IsPlayerHome)
                        {
                            TaleRecorder.RecordTale(TaleDefOf.CaravanRemoteMining, new object[]
                            {
                                this.pawn,
                                mineTarget.def.building.mineableThing
                            });
                        }
                        this.ReadyForNextToil();
                        return;
                    }
                    this.ResetTicksToPickHit();
                }
            };
            mine.defaultCompleteMode = ToilCompleteMode.Never;
            mine.WithProgressBar(TargetIndex.A, () => 1f - (float)this.MineTarget.HitPoints / (float)this.MineTarget.MaxHitPoints, false, -0.5f, false);
            mine.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            mine.activeSkill = (() => SkillDefOf.Mining);
            yield return mine;
            yield break;
        }

        private void ResetTicksToPickHit()
        {
            float num = this.pawn.GetStatValue(StatDefOf.MiningSpeed, true, -1);
            if (num < 0.6f && this.pawn.Faction != Faction.OfPlayer)
            {
                num = 0.6f;
            }
            this.ticksToPickHit = (int)Math.Round((double)(100f / num));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToPickHit, "ticksToPickHit", 0, false);
        }

        private int ticksToPickHit = -1000;

        private Effecter effecter;

        public const int BaseTicksBetweenPickHits = 100;

        private const int BaseDamagePerPickHit_NaturalRock = 80;

        private const int BaseDamagePerPickHit_NotNaturalRock = 40;

        private const float MinMiningSpeedFactorForNPCs = 0.6f;
    }
}
