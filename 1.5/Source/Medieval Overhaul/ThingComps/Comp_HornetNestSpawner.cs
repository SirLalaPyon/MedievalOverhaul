using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse.Sound;
using Verse;
using UnityEngine;

namespace MedievalOverhaul
{
    public class Comp_HornetNestSpawner : ThingComp
    {
        public int nextPawnSpawnTick = -1;
        public bool aggressive = true;
        public bool canSpawnPawns = true;
        //private CompCanBeDormant dormancyCompCached;

        private CompProperties_HornetNestSpawner Props => (CompProperties_HornetNestSpawner)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad || this.nextPawnSpawnTick != -1)
                return;
            this.SpawnInitialPawns();
        }

        private void SpawnInitialPawns()
        {
            int num = 0;
            while (num < this.Props.initialPawnCount && this.TrySpawnPawn(out Pawn _))
                ++num;
            this.CalculateNextPawnSpawnTick();
        }

        private void CalculateNextPawnSpawnTick() => this.CalculateNextPawnSpawnTick(this.Props.pawnSpawnIntervalDays.RandomInRange * 60000f);

        public void CalculateNextPawnSpawnTick(float delayTicks) => this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)((double)delayTicks / (1.0 * (double)Find.Storyteller.difficulty.enemyReproductionRateFactor));

        private bool TrySpawnPawn(out Pawn pawn)
        {
            int num = 0;
            foreach (string defName in this.Props.spawnablePawnKinds.Distinct<string>())
            {
                var thing = defName;
                num += this.parent.Map.listerThings.ThingsOfDef(ThingDef.Named(defName)).Count;
            }
            if (num < Props.maxPawnCount)
            {
                PawnKindDef named = DefDatabase<PawnKindDef>.GetNamed(this.Props.spawnablePawnKinds.RandomElement<string>(), false);
                if (named != null)
                {
                    Pawn pawnToCreate = PawnGenerator.GeneratePawn(named, Faction.OfInsects);
                    GenSpawn.Spawn((Thing)pawnToCreate, CellFinder.RandomClosewalkCellNear(this.parent.Position, this.parent.Map, this.Props.pawnSpawnRadius), this.parent.Map);
                    if (this.parent.Map != null)
                    {
                        Lord lord = (Lord)null;
                        if (this.parent.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfInsects).Any<Pawn>((Predicate<Pawn>)(p => p != pawnToCreate)))
                            lord = ((Pawn)GenClosest.ClosestThing_Global(this.parent.Position, (IEnumerable)this.parent.Map.mapPawns.SpawnedPawnsInFaction(Faction.DankPyon_Hornets), 30f, (Predicate<Thing>)(p => p != pawnToCreate && ((Pawn)p).GetLord() != null))).GetLord();
                        if (lord == null)
                            lord = LordMaker.MakeNewLord(Faction.OfInsects, (LordJob)new LordJob_DefendPoint(this.parent.Position, new float?(10f)), this.parent.Map);
                        lord.AddPawn(pawnToCreate);
                    }
                    pawn = pawnToCreate;
                    if (this.Props.spawnSound != null)
                        this.Props.spawnSound.PlayOneShot((SoundInfo)(Thing)this.parent);
                    return true;
                }
                pawn = (Pawn)null;
                return false;
            }
            canSpawnPawns = false;
            pawn = (Pawn)null;
            return false;
        }
        public override void CompTick()
        {
            if (this.parent.Spawned && this.nextPawnSpawnTick == -1)
                this.SpawnInitialPawns();
            if (!this.parent.Spawned || Find.TickManager.TicksGame < this.nextPawnSpawnTick)
                return;
            Pawn pawn;
            if (this.TrySpawnPawn(out pawn) && pawn.caller != null)
                pawn.caller.DoCall();
            this.CalculateNextPawnSpawnTick();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn pawn",
                    icon = TexCommand.ReleaseAnimals,
                    action = delegate ()
                    {
                        Pawn pawn;
                        this.TrySpawnPawn(out pawn);
                    }
                };
            }
            yield break;
        }
        public override string CompInspectStringExtra()
        {
            if (!canSpawnPawns)
                return (string)"DormantHiveNotReproducing".Translate();
            return this.canSpawnPawns ? (string)("HiveReproducesIn".Translate() + ": " + (this.nextPawnSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()) : (string)null;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick");
            Scribe_Values.Look<bool>(ref this.aggressive, "aggressive");
            Scribe_Values.Look<bool>(ref this.canSpawnPawns, "canSpawnPawns");
        }

    }
}