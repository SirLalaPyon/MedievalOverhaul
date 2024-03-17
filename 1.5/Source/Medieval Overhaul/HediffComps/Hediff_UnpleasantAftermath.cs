using System;
using System.Linq;
using RimWorld;
using Verse;

namespace DankPyon
{
	public class Hediff_UnpleasantAftermath : HediffWithComps
	{
		public override bool Visible
		{
			get
			{
				if (pawn.health.hediffSet.HasHediff(DankPyonDefOf.DankPyon_UnpleasantAftermath))
				{
					return false;
				}
				return base.Visible;
			}
		}
	}
}
