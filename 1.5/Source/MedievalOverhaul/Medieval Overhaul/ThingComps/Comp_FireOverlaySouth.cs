using RimWorld;
using Verse;
using UnityEngine;

namespace MedievalOverhaul
{
    public class Comp_FireOverlaySouth : CompFireOverlay
    {
		public override void PostDraw()
		{
			if ((refuelableCompCustom == null || refuelableCompCustom.HasFuel) && parent.Rotation == Rot4.North)
			{
				Vector3 drawPos = parent.DrawPos;
				drawPos.y += 3f / 74f;
				FireGraphic.Draw(drawPos, Rot4.North, parent);
			}
		}
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.refuelableCompCustom = this.parent.GetComp<CompRefuelableCustom>();
        }
        public override void CompTick()
		{
			if (((refuelableCompCustom == null || refuelableCompCustom.HasFuel) && parent.Rotation == Rot4.North) && startedGrowingAtTick < 0)
			{
				startedGrowingAtTick = GenTicks.TicksAbs;
			}
		}
        protected CompRefuelableCustom refuelableCompCustom;
    }
}
