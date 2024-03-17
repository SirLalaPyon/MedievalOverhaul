using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class Plant_SpawnerOnDestroy : Plant
    {
        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {
            Comp_PawnSpawnerOnDestroy comp = this.GetComp<Comp_PawnSpawnerOnDestroy>();
            if (comp != null && (!comp.Props.onlyIfHarvestable || comp.Props.onlyIfHarvestable && this.HarvestableNow))
                comp.PawnSpawnerWorker(by.Map);
            base.PlantCollected(by, plantDestructionMode);
        }
    }
}
