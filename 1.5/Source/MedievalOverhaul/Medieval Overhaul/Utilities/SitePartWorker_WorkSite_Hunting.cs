using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class SitePartWorker_WorkSite_Hunting : SitePartWorker_WorkSite
    {
        public override IEnumerable<PreceptDef> DisallowedPrecepts
        {
            get
            {
                return from p in DefDatabase<PreceptDef>.AllDefs
                       where p.disallowHuntingCamps
                       select p;
            }
        }

        public override PawnGroupKindDef WorkerGroupKind
        {
            get
            {
                return PawnGroupKindDefOf.Hunters;
            }
        }

        public override bool CanSpawnOn(int tile)
        {
            return Find.WorldGrid[tile].biome.animalDensity > BiomeDefOf.Desert.animalDensity && base.CanSpawnOn(tile);
        }

        public override IEnumerable<SitePartWorker_WorkSite.CampLootThingStruct> LootThings(int tile)
        {
            IEnumerable<ThingDef> enumerable = from a in Find.WorldGrid[tile].biome.AllWildAnimals
                                               where a?.RaceProps?.leatherDef is not null
                                               select a.RaceProps.leatherDef;
            float leatherWeight = 1f / (float)enumerable.Count<ThingDef>();
            foreach (ThingDef thing in enumerable)
            {
                if (thing is not null)
                {
                    if (HideUtility.IsHide(thing))
                    {
                        ThingDefCountClass thingDefCountClass = thing.butcherProducts[0];
                        ThingDef butcherDef = thingDefCountClass.thingDef;
                        yield return new SitePartWorker_WorkSite.CampLootThingStruct
                        {
                            thing = butcherDef,
                            thing2 = ThingDefOf.Pemmican,
                            weight = leatherWeight
                        };
                    }
                    else
                    {
                        yield return new SitePartWorker_WorkSite.CampLootThingStruct
                        {
                            thing = thing,
                            thing2 = ThingDefOf.Pemmican,
                            weight = leatherWeight
                        };
                    }
                }
                else
                {
                    yield return new SitePartWorker_WorkSite.CampLootThingStruct
                    {
                        thing = ThingDefOf.Pemmican,
                        weight = leatherWeight
                    };
                }

                
            }
            IEnumerator<ThingDef> enumerator = null;
            yield break;
        }
    }
}
