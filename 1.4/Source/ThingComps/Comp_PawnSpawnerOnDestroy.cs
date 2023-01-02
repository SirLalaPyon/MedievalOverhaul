using RimWorld;
using Verse;
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

        /// <summary>
        /// Should be fairly self explanatory
        /// Doesn't run PawnSpawnerWorker if the class of the parent is Plant_SpawnerOnDestroy, as that handles everything instead
        /// </summary>
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (parent is not Plant_SpawnerOnDestroy && !Props.disableCompDestroy)
            {
                PawnSpawnerWorker(previousMap);
            }
            base.PostDestroy(mode, previousMap);
        }

        public void PawnSpawnerWorker(Map previousMap)
        {
            if (Props.kindToSpawn != null && Rand.Chance(Props.chance))
            {
                int target = Props.numberToSpawn.RandomInRange;
                List<Pawn> pawns = new() { };
                for (int i = 0; i < target; i++)
                {
                    pawns.Add(PawnSpawner(previousMap));
                }
                if (Props.sendLetter && !pawns.NullOrEmpty())
                {
                    Find.LetterStack.ReceiveLetter(Props.letterLabel, Props.letterDescription, Props.letterDef, pawns);
                }
            }
        }

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
