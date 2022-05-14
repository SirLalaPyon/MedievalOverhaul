using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class BuildingLootableExtension : DefModExtension
    {
        // Loot stuff.
        public bool isRandom = false;
        public float lootChance = 1f;
        public string itemDefName = null;
        public IntRange lootCount = new (1, 2);
        public List<string> randomItems = null;
        public float effectSize = 1f;
        public SoundDef searchSound = null;
        public FleckDef searchEffect = null;

        // Enemy stuff.
        public List<string> enemysToSpawn = null;
        public int enemySpawnCount = 1;
        public float enemySpawnChance = 0.01f;

        // Empty graphic stuff.
        public string emptyGraphicPath;
    }
}
