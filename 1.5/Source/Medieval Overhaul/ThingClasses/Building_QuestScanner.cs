using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class Building_QuestScanner : Building
    {

        public bool CanWorkWithoutFuel
        {
            get
            {
                return this.refuelableComp == null;
            }
        }
        private CompRefuelable refuelableComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (QuestScriptDefOf.LongRangeMineralScannerLump == null)
            {
                this.Destroy(DestroyMode.Refund);
                Log.Error("Long Range Mineral Lump Quest has been removed. Destroying and refunding building. ");
            }
            this.refuelableComp = base.GetComp<CompRefuelable>();
        }

        public virtual void UsedThisTick()
        {
            if (this.refuelableComp != null)
            {
                this.refuelableComp.Notify_UsedThisTick();
            }
        }
    }
}
