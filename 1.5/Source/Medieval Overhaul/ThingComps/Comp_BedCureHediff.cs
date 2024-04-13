using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class Comp_BedCureHediff : ThingComp
    {
        public Building_Bed building_Bed
        {
            get
            {
                return this.parent as Building_Bed;
            }
        }
        public CompProperties_BedCureHediff Props
        {
            get
            {
                return (CompProperties_BedCureHediff)this.props;
            }
        }
        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(Props.intervalTick))
            {
                if (this.parent != null)
                {
                    if (building_Bed.AnyOccupants)
                    {
                        Pawn pawn = this.building_Bed.GetCurOccupant(0);
                        if (pawn != null)
                        {
                            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.cureHediff);
                            if (hediff is null)
                                return;
                            hediff.Severity -= Props.cureSeverity;
                        }
                        Pawn pawn2 = this.building_Bed.GetCurOccupant(1);
                        if (pawn2 != null)
                        {
                            var hediff = pawn2.health.hediffSet.GetFirstHediffOfDef(Props.cureHediff);
                            if (hediff is null)
                                return;
                            hediff.Severity -= Props.cureSeverity;
                        }
                    }
                }
            }
        }
    }
}
