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

        /* Disables spawning if destroyed by specific damage types
         * You probably don't want angry tree people spawning from trees that die of old age
         */
        public bool notRot = true;
        public bool notFlame = true;
        public bool notDeterioration = true;
        /* disables spawning if there is currently a toxic fallout active */
        public bool notToxicFallout = true;
    }
}
