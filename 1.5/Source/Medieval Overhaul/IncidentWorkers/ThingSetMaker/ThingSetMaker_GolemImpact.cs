using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Reflection;
using UnityRandom = UnityEngine.Random;
using UnityEngine.Assertions.Must;

namespace MedievalOverhaul
{
    public class ThingSetMaker_GolemImpact : ThingSetMaker
    {
        public static void Reset()
        {
            ThingSetMaker_GolemImpact.nonSmoothedMineables.Clear();
            ThingSetMaker_GolemImpact.nonSmoothedMineables.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading
                                                                  where x.mineable && x != ThingDefOf.CollapsedRocks && x != ThingDefOf.RaisedRocks && !x.IsSmoothed
                                                                  select x);
        }
        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            IntRange? countRange = parms.countRange;
            int randomInRange = ((countRange == null) ? ThingSetMaker_GolemImpact.MineablesCountRange : countRange.Value).RandomInRange;
            ThingDef def = FindRandomMineableDef();
            for (int i = 0; i < randomInRange; i++)
            {
                Building building = (Building)ThingMaker.MakeThing(def, null);
                building.canChangeTerrainOnDestroyed = false;
                outThings.Add(building);
            }
            GolemSpawn golemSpawn = GolemSpawn.Get((Def)def);
            PawnKindDef pawnKind = golemSpawn.kindDef;
            Map mapPlayerHome = null;
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome)
                {
                    mapPlayerHome = maps[i];
                }
            }

            float num = StorytellerUtility.DefaultThreatPointsNow(mapPlayerHome);
            int num2 = GenMath.RoundRandom(num / pawnKind.combatPower);
            int max = Rand.RangeInclusive(5, 20);
            num2 = Mathf.Clamp(num2, 1, max);
            for (int i = 0; i < num2; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, null);
                pawn.SetFaction(Faction.OfMechanoids);
                outThings.Add(pawn);
            }


        }
        public ThingDef FindRandomMineableDef()
        {

            mineAbles.Add(MedievalOverhaulDefOf.DankPyon_MineableIron);
            mineAbles.Add(MedievalOverhaulDefOf.DankPyon_MineableGold);
            mineAbles.Add(ThingDefOf.MineableSteel);
            mineAbles.Add(MedievalOverhaulDefOf.MineableSilver);
            System.Random random = new System.Random();
            int randomIndex = random.Next(0, mineAbles.Count);
            ThingDef thingDef = mineAbles[randomIndex];
            return thingDef;
        }
        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return ThingSetMaker_GolemImpact.nonSmoothedMineables;
        }
        public static List<ThingDef> nonSmoothedMineables = new List<ThingDef>();
        public static List<ThingDef> mineAbles = new List<ThingDef>();

        public static readonly IntRange MineablesCountRange = new IntRange(8, 20);

        private const float PreciousMineableMarketValue = 5f;
    }
}
