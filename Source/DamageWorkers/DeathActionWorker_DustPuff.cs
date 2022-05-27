using RimWorld;
using Verse;
using UnityEngine;
using DankPyon;

namespace DankPyon
{
	public class DeathActionWorker_DustPuff : DeathActionWorker_SmallExplosion
	{
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

		public override bool DangerousInMelee => true;

		public override void PawnDied(Corpse corpse)
		{
			GenExplosion.DoExplosion(corpse.Position, corpse.Map, 1.9f, DankPyonDefOf.DankPyon_SchratCollapse, corpse.InnerPawn);
		}
	}
}
