using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class Building_WorkTableCustom : Building, IBillGiver, IBillGiverWithTickAction
    {
        public bool CanWorkWithoutPower
        {
            get
            {
                return this.powerComp == null || this.def.building.unpoweredWorkTableWorkSpeedFactor > 0f;
            }
        }

        public bool CanWorkWithoutFuel
        {
            get
            {
                return this.refuelableComp == null;
            }
        }

        public Building_WorkTableCustom()
        {
            this.billStack = new BillStack(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<BillStack>(ref this.billStack, "billStack", new object[]
            {
                this
            });
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.refuelableComp = base.GetComp<CompRefuelableCustom>();
            this.breakdownableComp = base.GetComp<CompBreakdownable>();
            this.moteEmitterComp = base.GetComp<CompMoteEmitter>();
            foreach (Bill bill in this.billStack)
            {
                bill.ValidateSettings();
            }
        }

        public virtual void UsedThisTick()
        {
            if (this.refuelableComp != null)
            {
                this.refuelableComp.Notify_UsedThisTick();
            }
            if (this.moteEmitterComp != null)
            {
                if (!this.moteEmitterComp.MoteLive)
                {
                    this.moteEmitterComp.Emit();
                }
                this.moteEmitterComp.Maintain();
            }
        }

        public BillStack BillStack
        {
            get
            {
                return this.billStack;
            }
        }

        public IntVec3 BillInteractionCell
        {
            get
            {
                return this.InteractionCell;
            }
        }

        public IEnumerable<IntVec3> IngredientStackCells
        {
            get
            {
                return GenAdj.CellsOccupiedBy(this);
            }
        }

        public bool CurrentlyUsableForBills()
        {
            return this.UsableForBillsAfterFueling() && (this.CanWorkWithoutPower || (this.powerComp != null && this.powerComp.PowerOn)) && (this.CanWorkWithoutFuel || (this.refuelableComp != null && this.refuelableComp.HasFuel));
        }

        public bool UsableForBillsAfterFueling()
        {
            return (this.CanWorkWithoutPower || (this.powerComp != null && this.powerComp.PowerOn)) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
        }

        public virtual void Notify_BillDeleted(Bill bill)
        {
        }

        public BillStack billStack;

        private CompPowerTrader powerComp;

        private CompRefuelableCustom refuelableComp;

        private CompBreakdownable breakdownableComp;

        private CompMoteEmitter moteEmitterComp;

    }
}
