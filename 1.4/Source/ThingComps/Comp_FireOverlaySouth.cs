using RimWorld;
using Verse;
using UnityEngine;

namespace DankPyon_MedievalOverhaul
{
    public class Comp_FireOverlaySouth : CompFireOverlay
    {
		public override void PostDraw()
		{
			if ((refuelableComp == null || refuelableComp.HasFuel) && parent.Rotation == Rot4.North)
			{
				Vector3 drawPos = parent.DrawPos;
				drawPos.y += 3f / 74f;
				FireGraphic.Draw(drawPos, Rot4.North, parent);
			}
		}
		public override void CompTick()
		{
			if (((refuelableComp == null || refuelableComp.HasFuel) && parent.Rotation == Rot4.North) && startedGrowingAtTick < 0)
			{
				startedGrowingAtTick = GenTicks.TicksAbs;
			}
		}
	}
}
