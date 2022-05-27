using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using Verse.Sound;

namespace MedievalOverhaul
{
    class Comp_PawnSpawnerOnDestroy : ThingComp
	{
		public CompProperties_PawnSpawnerOnDestroy Props
		{
			get
			{
				return (CompProperties_PawnSpawnerOnDestroy)this.props;
			}
		}

		/* ==========[Used to check if the thing is taking specific damag types]========== */

		public bool takingAgeDamage = false;

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			takingAgeDamage = (dinfo.Def == DamageDefOf.Rotting && Props.notRot) || (dinfo.Def == DamageDefOf.Deterioration && Props.notDeterioration) || (dinfo.Def == DamageDefOf.Flame && Props.notFlame);
			base.PostPostApplyDamage(dinfo, totalDamageDealt);
		}

        /* ==========[Checks n shit]========== */

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (takingAgeDamage || (Props.notToxicFallout && previousMap.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout)))
            {
                base.PostDestroy(mode, previousMap);
                return;
            }
            if (Props.kindToSpawn != null && Rand.Chance(Props.chance))
            {
                int target = Props.numberToSpawn.RandomInRange;
                List<Pawn> pawns = new List<Pawn> { };
                for (int i = 0; i < target; i++)
                {
                    pawns.Add(PawnSpawner(previousMap));
                }
                if (Props.sendLetter && !pawns.NullOrEmpty())
                {
                    Find.LetterStack.ReceiveLetter(Props.letterLabel, Props.letterDescription, Props.letterDef, pawns);
                }
            }

            base.PostDestroy(mode, previousMap);
        }

        /* ==========[The actual spawning part]========== */

        public Pawn PawnSpawner(Map map)
        {
            Pawn newPawn = PawnGenerator.GeneratePawn(Props.kindToSpawn, Props.spawnAsPlayerFaction ? Faction.OfPlayer : Props.faction != null && FactionUtility.DefaultFactionFrom(Props.faction) != null ? FactionUtility.DefaultFactionFrom(Props.faction) : null);

            GenSpawn.Spawn(newPawn, parent.Position, map);

            if (Props.soundDef != null)
            {
                SoundDef sound = Props.soundDef;
                sound.PlayOneShot(new TargetInfo(parent.Position, map, false));
            }
            if (Props.mentalStateDef != null)
            {
                newPawn.mindState.mentalStateHandler.TryStartMentalState(Props.mentalStateDef);
            }
            return newPawn;
        }
    }
}
