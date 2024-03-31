using RimWorld;
using Verse;
using UnityEngine;
using MedievalOverhaul;
using Verse.AI.Group;
using System;
using static UnityEngine.GraphicsBuffer;

namespace MedievalOverhaul
{
	public class DeathActionWorker_DarkSchratSplit : DeathActionWorker
	{
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

		public override bool DangerousInMelee => true;

        public override void PawnDied(Corpse corpse, Lord prevLord)
		{
			if (corpse.Map != null)
			{
                GenExplosion.DoExplosion(corpse.Position, corpse.Map, 3f, MedievalOverhaulDefOf.DankPyon_SchratCollapse, corpse.InnerPawn, -1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, true, null, null, null, true, 1f, 0f, true, null, 1f, null, null);
                int range = 2;
                for (int i = 0; i < range; i++)
				{
                    IntVec3 intVec3 = CellFinder.RandomClosewalkCellNear(corpse.Position, corpse.Map, 3);
                    Pawn pawn = PawnGenerator.GeneratePawn(MedievalOverhaulDefOf.DankPyon_SchratDark_Sapling, null);
                    pawn.ageTracker.AgeBiologicalTicks = 60;
                    GenSpawn.Spawn((Thing)pawn, intVec3, corpse.Map, Rot4.Random, WipeMode.Vanish, false);
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, false, null, false, false, false);
                }
            }
        }
	}
}
