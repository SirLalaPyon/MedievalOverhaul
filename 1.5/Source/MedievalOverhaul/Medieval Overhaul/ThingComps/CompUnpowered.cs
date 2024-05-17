using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
	public class CompUnpowered : CompPowerTrader
	{
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			yield break;
		}

		public override string CompInspectStringExtra()
		{
			return null;
		}
	}
}
