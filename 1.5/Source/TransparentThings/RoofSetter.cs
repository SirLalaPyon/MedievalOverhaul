using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TransparentThings
{
    public class RoofSetter : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad)
                return;
            foreach (IntVec3 c in this.OccupiedRect())
                map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach (IntVec3 c in this.OccupiedRect())
                this.Map.roofGrid.SetRoof(c, (RoofDef)null);
            base.Destroy(mode);
        }
    }
}
