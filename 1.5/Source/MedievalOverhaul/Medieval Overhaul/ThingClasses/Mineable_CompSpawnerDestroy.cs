using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{ 
    public class Mineable_CompSpawnerDestroy : Building
    {
        private float yieldPct;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.yieldPct, "yieldPct", 0f, false);
        }
        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            if (absorbed)
            {
                return;
            }
            if (this.def.building.mineableThing != null && this.def.building.mineableYieldWasteable && dinfo.Def == DamageDefOf.Mining && dinfo.Instigator != null && dinfo.Instigator is Pawn)
            {
                this.Notify_TookMiningDamage(GenMath.RoundRandom(dinfo.Amount), (Pawn)dinfo.Instigator);
            }
            absorbed = false;
        }
        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (dinfo != null && dinfo.Value.Instigator != null && dinfo.Value.Def != DamageDefOf.Mining)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.DestroyedMineable, dinfo.Value.Instigator.Named(HistoryEventArgsNames.Doer)), true);
            }
            base.Kill(dinfo, exactCulprit);
        }

        public void DestroyMined(Pawn pawn)
        {
            Map map = this.Map;
            base.Destroy(DestroyMode.KillFinalize);
            this.TrySpawnYield(map, true, pawn);
            if (pawn == null)
                return;
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.Mined, pawn.Named(HistoryEventArgsNames.Doer)));
        }

        public override void Destroy(DestroyMode mode)
        {
            Map map = base.Map;
            base.Destroy(mode);
            if (mode == DestroyMode.KillFinalize)
            {
                this.TrySpawnYield(map, false, null);
            }
        }

        private void TrySpawnYield(Map map, bool moteOnWaste, Pawn pawn)
        {
            
            if (this.def.building.mineableThing == null || (double)Rand.Value > (double)this.def.building.mineableDropChance)
                return;
            int num = Mathf.Max(1, this.def.building.EffectiveMineableYield);
            if (this.def.building.mineableYieldWasteable)
                num = Mathf.Max(1, GenMath.RoundRandom((float)num * this.yieldPct));
            Thing thing = ThingMaker.MakeThing(this.def.building.mineableThing);
            thing.stackCount = num;
            GenPlace.TryPlaceThing(thing, this.Position, map, ThingPlaceMode.Near, new Action<Thing, int>(ForbidIfNecessary));
            Comp_PawnSpawnerOnDestroy comp = this.GetComp<Comp_PawnSpawnerOnDestroy>();
            if (comp != null)
            {
                comp.PawnSpawnerWorker(map);
            }
            void ForbidIfNecessary(Thing thing, int count)
            {
                if (pawn != null && pawn.Faction == Faction.OfPlayer || !thing.def.EverHaulable || thing.def.designateHaulable)
                    return;
                thing.SetForbidden(true, false);
            }

        }
        public void Notify_TookMiningDamage(int amount, Pawn miner)
        {
            float num = (float)Mathf.Min(amount, this.HitPoints) / (float)base.MaxHitPoints;
            this.yieldPct += num * miner.GetStatValue(StatDefOf.MiningYield, true, -1);
        }
    }
}
