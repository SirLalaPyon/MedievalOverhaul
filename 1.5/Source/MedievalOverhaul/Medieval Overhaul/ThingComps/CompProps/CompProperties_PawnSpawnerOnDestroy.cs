using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    class CompProperties_PawnSpawnerOnDestroy : CompProperties
    {
        public CompProperties_PawnSpawnerOnDestroy()
        {
            this.compClass = typeof(Comp_PawnSpawnerOnDestroy);
        }

        public PawnKindDef kindToSpawn = null;
        public bool spawnAsPlayerFaction = true;
        public FactionDef faction = null;
        public MentalStateDef mentalStateDef = null;
        public IntRange numberToSpawn;
        public SoundDef soundDef = null;
        public float chance = 1f;

        /* For sending a letter when spawning happens */
        public bool sendLetter = true;
        public string letterLabel = null;
        public string letterDescription = null;
        public LetterDef letterDef = null;

        public bool onlyIfHarvestable = false;
        //remvove in 1.5
        public bool disableCompDestroy = false;
    }
}
