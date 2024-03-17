using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ESCP_FuelExtension
{
    public class CompStoreFuelThing : ThingComp
    {
        public ThingDef fuelUsed;
        private ThingFilter allowedFuelFilter;

        public ThingFilter AllowedFuelFilter => this.allowedFuelFilter;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled)
                return;
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.allowedFuelFilter != null)
                return;
            this.allowedFuelFilter = new ThingFilter();
            this.allowedFuelFilter.CopyAllowancesFrom(this.parent.GetComp<CompRefuelable>().Props.fuelFilter);
        }

        public override void PostExposeData()
        {
            if (Utility_OnStartup.LWMFuelFilterIsEnabled)
                return;
            base.PostExposeData();
            Scribe_Defs.Look<ThingDef>(ref this.fuelUsed, "fuelUsed");
            Scribe_Deep.Look<ThingFilter>(ref this.allowedFuelFilter, "allowedFuelFilter");
        }

        public override string CompInspectStringExtra() => Utility_OnStartup.LWMFuelFilterIsEnabled || this.parent.GetComp<CompRefuelable>() == null ? (string)null : (string)(!this.parent.GetComp<CompRefuelable>().HasFuel || this.fuelUsed == null ? (TaggedString)(string)null : "ESCP_Tools_FuelExtension_CurrentFuel".Translate((NamedArgument)this.fuelUsed.label));
    }
}
