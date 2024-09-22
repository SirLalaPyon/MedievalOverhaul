using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class Building_WorkTableCustom : Building_WorkTable
    {
        public new bool CanWorkWithoutFuel
        {
            get
            {
                return this.fuelComp == null;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.fuelComp = base.GetComp<CompRefuelableCustom>();
        }

        public override void UsedThisTick()
        {
            if (this.fuelComp != null)
            {
                this.fuelComp.Notify_UsedThisTick();
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

        public new bool CurrentlyUsableForBills()
        {
            Log.Message("Are we proccing");
            return this.UsableForBillsAfterFueling() && (this.CanWorkWithoutPower || (this.powerComp != null && this.powerComp.PowerOn)) && (this.CanWorkWithoutFuel || (this.fuelComp != null && this.fuelComp.HasFuel));
        }
        public new bool UsableForBillsAfterFueling()
        {
            return (this.CanWorkWithoutPower || (this.powerComp != null && this.powerComp.PowerOn)) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
        }

        private CompRefuelableCustom fuelComp;

    }
}
