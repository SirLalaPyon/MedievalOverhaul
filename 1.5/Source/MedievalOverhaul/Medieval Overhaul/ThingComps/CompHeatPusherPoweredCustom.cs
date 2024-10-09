using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class CompHeatPusherPoweredCustom : CompHeatPusherPowered
    {

        public override bool ShouldPushHeatNow
        {
            get
            {
                return base.ShouldPushHeatNow && FlickUtility.WantsToBeOn(this.parent) && (this.powerComp == null || this.powerComp.PowerOn) && (this.refuelableCompCustom == null || this.refuelableCompCustom.HasFuel) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.powerComp = this.parent.GetComp<CompPowerTrader>();
            this.refuelableCompCustom = this.parent.GetComp<CompRefuelableCustom>();
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
        }
        protected CompRefuelableCustom refuelableCompCustom;
    }
}
