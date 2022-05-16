using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    // Need the constructor for GraphicEmpty initialization on game load.
    public class Building_Lootable : Building_Crate, IOpenable
    {
        private bool Searched;
        private bool RespawningAfterLoad;
        private Graphic emptyColoredGraphic;

        private void DetermineEmptyGraphicColor()
        {
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();
            if (lootableExt == null)
                return;
            emptyColoredGraphic = lootableExt.emptyGraphicData?.GraphicColoredFor(this);
        }

        public override void PostMake()
        {
            base.PostMake();
            DetermineEmptyGraphicColor();
        }

        /// <summary>
        /// Can only be opened if the contents inside are not known.
        /// Contents not known by default.
        /// </summary>
        public override bool CanOpen => !contentsKnown;

        /// <summary>
        /// Randomly generates and stores items for a player to find via looting, stored inside the innerContainer.
        /// Contents of the ThingOwner are unknown until the building is searched.
        /// </summary>
        private void LootGeneration()
        {
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();

            if (!RespawningAfterLoad)
            {
                if (!Searched)
                {
                    if (Rand.Chance(lootableExt.enemySpawnChance))
                    {
                        ThingDef lootableTD;
                        // Random search results.
                        if (lootableExt.isRandom == true)
                        {
                            lootableTD = DefDatabase<ThingDef>.GetNamedSilentFail(lootableExt.randomItems.RandomElement());
                            if (lootableTD != null)
                            {
                                Thing thing1 = ThingMaker.MakeThing(lootableTD, null);
                                thing1.stackCount = lootableExt.lootCount.RandomInRange;
                                innerContainer.TryAdd(thing1, lootableExt.lootCount.RandomInRange);
                            }
                        }
                        // Non-random search results.
                        else
                        {
                            lootableTD = DefDatabase<ThingDef>.GetNamedSilentFail(lootableExt.itemDefName);
                            if (lootableTD != null)
                            {
                                Thing thing2 = ThingMaker.MakeThing(lootableTD, null);
                                thing2.stackCount = lootableExt.lootCount.RandomInRange;
                                innerContainer.TryAdd(thing2, lootableExt.lootCount.RandomInRange);
                            }
                        }
                    }
                    contentsKnown = false;
                }
            }
        }

        /// <summary>
        /// Randomly generates and stores a pawn for a player to find via looting, stored inside the innerContainer.
        /// Contents of the ThingOwner are unknown until the building is searched.
        /// </summary>
        private void EnemyGeneration()
        {
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();

            if (!RespawningAfterLoad)
            {
                if (!Searched)
                {
                    if (Rand.Chance(lootableExt.enemySpawnChance))
                    {
                        PawnGenerationRequest request = new(PawnKindDef.Named(lootableExt.enemysToSpawn.RandomElement()),
                            null, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false);
                        Pawn pawn = PawnGenerator.GeneratePawn(request);
                        innerContainer.TryAdd(pawn, lootableExt.enemySpawnCount);
                    }
                    contentsKnown = false;
                }
            }
        }

        /// <summary>
        /// Random loot generation. Loot is stored in buildings' innerContainer.
        /// Contents unknown until opened by a pawn.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LootGeneration();
            EnemyGeneration();
        }

        /// <summary>
        /// Opens the innerContainer and emits a visual effect.
        /// Once called, the contents of the container are known.
        /// </summary>
        public override void Open()
        {
            base.Open();
            contentsKnown = true;
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();
            // Now that the contents are known, this building cannot be searched again.

            if (lootableExt.searchEffect != null)
            {
                FleckCreationData fCD = FleckMaker.GetDataStatic(DrawPos, Map, lootableExt.searchEffect, lootableExt.effectSize);
                fCD.rotationRate = Rand.RangeInclusive(-240, 240);
                fCD.velocitySpeed = Rand.Range(0.1f, 0.8f);
                Map.flecks.CreateFleck(fCD);
            }
        }

        /// <summary>
        /// Once innerContainer is opened, contents thrown out to valid cell.
        /// </summary>
        public override void EjectContents()
        {
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();

            List<Pawn> pawns = innerContainer
                .Where(thing => thing is Pawn)
                .Cast<Pawn>()
                .ToList();

            innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near, nearPlaceValidator: c => c.GetEdifice(Map) == null);
            foreach (Pawn pawn in pawns)
            {
                if (pawn.RaceProps.Animal && lootableExt.hostileEnemy == true)
                {
                    pawn.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                }
                else if (!pawn.RaceProps.Animal && lootableExt.hostileEnemy == true)
                {
                    pawn.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.Berserk);
                }
            }
            contentsKnown = true;
        }

        /// <summary>
        /// Changes the building's graphic after its been searched for loot.
        /// Regardless if it contained anything or not.
        /// </summary>
        public override Graphic Graphic
        {
            get
            {
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, true, false);

                if (contentsKnown == true && emptyColoredGraphic != null)
                {
                    return emptyColoredGraphic;
                }

                return DefaultGraphic;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref contentsKnown, "contentsKnown", defaultValue: false);
            Scribe_Values.Look(ref Searched, "Searched", defaultValue: false);
        }
    }
}
