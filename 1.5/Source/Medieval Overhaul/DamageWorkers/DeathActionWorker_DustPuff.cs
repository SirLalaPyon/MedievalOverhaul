﻿using RimWorld;
using Verse;
using UnityEngine;
using MedievalOverhaul;
using Verse.AI.Group;

namespace MedievalOverhaul
{
	public class DeathActionWorker_DustPuff : DeathActionWorker_SmallExplosion
	{
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

		public override bool DangerousInMelee => true;

		public override void PawnDied(Corpse corpse, Lord prevLord)
		{
			GenExplosion.DoExplosion(corpse.Position, corpse.Map, 1.9f, MedievalOverhaulDefOf.DankPyon_SchratCollapse, corpse.InnerPawn);
		}
	}
}