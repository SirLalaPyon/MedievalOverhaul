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
        public FactionDef faction;
        public MentalStateDef mentalStateDef;
        public IntRange numberToSpawn;
        public SoundDef soundDef;
        public float chance = 1f;

        /* For sending a letter when spawning happens */
        public bool sendLetter = true;
        public string letterLabel;
        public string letterDescription;
        public LetterDef letterDef;

        public bool onlyIfHarvestable = false;
        //remvove in 1.5
        public bool disableCompDestroy = false;
    }
}
