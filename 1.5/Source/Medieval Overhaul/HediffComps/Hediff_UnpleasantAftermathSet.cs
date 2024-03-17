using System;
using System.Linq;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    public class Hediff_UnpleasantAftermathSet : Hediff_High
    {
		private const int HangoverCheckInterval = 300;

		public override void Tick()
		{
			base.Tick();
			if (pawn.IsHashIntervalTick(300) && HangoverSusceptible(pawn))
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(MedievalOverhaulDefOf.DankPyon_UnpleasantAftermath);
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity = 1f;
					return;
				}
				firstHediffOfDef = HediffMaker.MakeHediff(MedievalOverhaulDefOf.DankPyon_UnpleasantAftermath, pawn);
				firstHediffOfDef.Severity = 1f;
				pawn.health.AddHediff(firstHediffOfDef);
			}
		}

		private bool HangoverSusceptible(Pawn pawn)
		{
			return true;
		}
	}
}
