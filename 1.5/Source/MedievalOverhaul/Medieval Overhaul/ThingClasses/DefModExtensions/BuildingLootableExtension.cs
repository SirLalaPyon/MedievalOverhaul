using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class BuildingLootableExtension : DefModExtension
    {
        // Loot stuff.
        public bool isRandom = true;
        public bool isDestroyed = false;
        public float lootChance = 1f;
        public string itemDefName = null;
        public IntRange lootCount = new (1, 2);
        public List<string> randomItems = new ();
        public float effectSize = 1f;
        public SoundDef searchSound = null;
        public FleckDef searchEffect = null;

        // Enemy stuff.
        public List<string> enemysToSpawn = new();
        public bool hostileEnemy = false;
        public int enemySpawnCount = 1;
        public float enemySpawnChance = 0.01f;
        public FactionDef faction;
        public bool spawnAsPlayerFaction;

        // Empty graphic stuff.
        public GraphicData emptyGraphicData;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if (emptyGraphicData == null)
            {
                yield return "If using an empty graphic, provide the required emptyGraphicData";
            }
            if (randomItems == null)
            {
                yield return "If <isRandom> is set to true, list multiple itemDefName's";
            }
        }
    }
}
