using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Unity.Mathematics;
using System.Security.Cryptography;

namespace MedievalOverhaul
{
    public class ThingSetMaker_GolemImpact : ThingSetMaker
    {
        public static void Reset()
        {
            ThingSetMaker_GolemImpact.golemMeteor.Clear();
            ThingSetMaker_GolemImpact.golemMeteor.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading
                                                                  where x.mineable && x != ThingDefOf.CollapsedRocks && x != ThingDefOf.RaisedRocks && !x.IsSmoothed
                                                                  select x);
        }

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            int randomInRange = (parms.countRange ?? ThingSetMaker_GolemImpact.MineablesCountRange).RandomInRange;
            ThingDef def = this.FindRandomMineableDef();
            for (int i = 0; i < randomInRange; i++)
            {
                Building building = (Building)ThingMaker.MakeThing(def, null);
                building.canChangeTerrainOnDestroyed = false;
                outThings.Add(building);
            }
        }

        private ThingDef FindRandomMineableDef()
        {
            float value = Rand.Value;
            if (value < 0.4f)
            {
                return (from x in ThingSetMaker_GolemImpact.golemMeteor
                        where !x.building.isResourceRock
                        select x).RandomElement<ThingDef>();
            }
            if (value < 0.75f)
            {
                return (from x in ThingSetMaker_GolemImpact.golemMeteor
                        where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue < 5f
                        select x).RandomElement<ThingDef>();
            }
            return (from x in ThingSetMaker_GolemImpact.golemMeteor
                    where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= 5f
                    select x).RandomElement<ThingDef>();


        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return ThingSetMaker_GolemImpact.golemMeteor;
        }

        public static List<ThingDef> golemMeteor = new List<ThingDef>();

        public static readonly IntRange MineablesCountRange = new IntRange(8, 20);

        private const float PreciousMineableMarketValue = 5f;

    }
}
