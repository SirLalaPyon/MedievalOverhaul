using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    // Need the constructor for GraphicEmpty initialization on game load.
    public class Building_Lootable : Building_Casket, IThingHolder, IOpenable
    {
        private Graphic emptyColoredGraphic;

        private void DetermineEmptyGraphicColor()
        {
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();
            if (lootableExt == null)
                return;
            emptyColoredGraphic = lootableExt.emptyGraphicData?.GraphicColoredFor(this);
        }

        private Graphic EmptyColoredGraphic
        {
            get
            {
                if (Scribe.mode != LoadSaveMode.Inactive)
                {
                    return null;
                }
                if (emptyColoredGraphic == null)
                {
                    DetermineEmptyGraphicColor();
                }
                return emptyColoredGraphic;
            }
        }

        /// <summary>
        /// Can only be opened if the contents inside are not known.
        /// Contents not known by default.
        /// </summary>
        public override bool CanOpen
        {
            get
            {
                return this.HasAnyContents;
            }
        }

        public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetMultiSelectFloatMenuOptions(selPawns))
            {
                yield return floatMenuOption;
            }
            IEnumerator<FloatMenuOption> enumerator = null;
            if (!this.CanOpen)
            {
                yield break;
            }
            Building_Lootable.tmpCanReach.Clear();
            Building_Lootable.tmpCanOpen.Clear();
            for (int i = 0; i < selPawns.Count; i++)
            {
                if (selPawns[i].RaceProps.Humanlike)
                {
                    if (selPawns[i].CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    {
                        Building_Lootable.tmpCanReach.Add(selPawns[i]);
                    }
                    if (this.IsCapableOfOpening(selPawns[i]))
                    {
                        Building_Lootable.tmpCanOpen.Add(selPawns[i]);
                    }
                }
            }
            if (Building_Lootable.tmpCanReach.Count == 0)
            {
                yield return new FloatMenuOption("CannotOpen".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            if (Building_Lootable.tmpCanOpen.Count == 0)
            {
                yield return new FloatMenuOption("CannotOpen".Translate(this.Label) + ": " + "Incapable".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            Building_Lootable.tmpAllowedPawns.Clear();
            for (int i = 0; i < tmpCanReach.Count; i++)
            {
                if (tmpCanOpen.Contains(tmpCanReach[i]))
                {
                    tmpAllowedPawns.Add(tmpCanReach[i]);
                }
            }
            if (tmpAllowedPawns.Count > 0)
            {
                yield return new FloatMenuOption("Open".Translate(this), delegate ()
                {
                    Building_Lootable.tmpAllowedPawns[0].jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Open, this), new JobTag?(JobTag.Misc), false);
                    for (int i = 1; i < Building_Lootable.tmpAllowedPawns.Count; i++)
                    {
                        FloatMenuMakerMap.PawnGotoAction(base.Position, Building_Lootable.tmpAllowedPawns[i], RCellFinder.BestOrderedGotoDestNear(base.Position, Building_Lootable.tmpAllowedPawns[i], null));
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            yield break;
        }

        /// <summary>
        /// Randomly generates and stores items for a player to find via looting, stored inside the innerContainer.
        /// Contents of the ThingOwner are unknown until the building is searched.
        /// </summary>
        private void LootGeneration()
        {
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();

            if (!contentsKnown)
            {
                if (Rand.Chance(lootableExt.lootChance))
                {
                    ThingDef lootableTD;
                    // Random search results.
                    if (lootableExt.isRandom == true)
                    {
                        lootableTD = DefDatabase<ThingDef>.GetNamedSilentFail(lootableExt.randomItems.RandomElement());
                        if (lootableTD != null)
                        {
                            Thing t1 = ThingMaker.MakeThing(lootableTD, GenStuff.RandomStuffFor(lootableTD));
                            t1.stackCount = lootableExt.lootCount.RandomInRange;
                            innerContainer.TryAdd(t1, lootableExt.lootCount.RandomInRange);
                        }
                    }
                    // Non-random search results.
                    else
                    {
                        lootableTD = DefDatabase<ThingDef>.GetNamedSilentFail(lootableExt.itemDefName);
                        if (lootableTD != null)
                        {
                            Thing t2 = ThingMaker.MakeThing(lootableTD, GenStuff.RandomStuffFor(lootableTD));
                            t2.stackCount = lootableExt.lootCount.RandomInRange;
                            innerContainer.TryAdd(t2, lootableExt.lootCount.RandomInRange);
                        }
                    }
                }
                contentsKnown = false;
            }
        }

        /// <summary>
        /// Randomly generates and stores a pawn for a player to find via looting, stored inside the innerContainer.
        /// Contents of the ThingOwner are unknown until the building is searched.
        /// </summary>
        private void EnemyGeneration()
        {
            BuildingLootableExtension lootableExt = def.GetModExtension<BuildingLootableExtension>();

            if (!contentsKnown)
            {
                //if (Rand.Chance(lootableExt.enemySpawnChance))
                if (Rand.Chance(lootableExt.enemySpawnChance))
                {
                   Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDef.Named(lootableExt.enemysToSpawn.RandomElement()), lootableExt.spawnAsPlayerFaction ? Faction.OfPlayer : lootableExt.faction != null && FactionUtility.DefaultFactionFrom(lootableExt.faction) != null ? FactionUtility.DefaultFactionFrom(lootableExt.faction) : null, PawnGenerationContext.NonPlayer, -1, false, false, false, true, false, 1f, false, true, false, true, true, false, true, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, true, false, false, -1, 0, false));
                    innerContainer.TryAdd(pawn, lootableExt.enemySpawnCount);

                    if (pawn.kindDef.defName.Contains("Empire_"))
                    {
                        pawn.SetFaction(Faction.OfEmpire);
                    }
                }
                contentsKnown = false;
            }
        }

        /// <summary>
        /// Random loot generation. Loot is stored in buildings' innerContainer.
        /// Contents unknown until opened by a pawn.
        /// </summary>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!contentsKnown && !respawningAfterLoad)
            {
                LootGeneration();
                EnemyGeneration();
            }
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

            Map.mapDrawer.MapMeshDirty(Position, 1, true, false);

            if(lootableExt.isDestroyed == true)
            {
                base.Destroy();
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

            innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near, null,null, true);
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
                if (contentsKnown == true && EmptyColoredGraphic != null)
                {
                    return EmptyColoredGraphic;
                }

                return DefaultGraphic;
            }
        }
        private bool IsCapableOfOpening(Pawn pawn)
        {
            return !pawn.IsMutant && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }

        private static List<Pawn> tmpCanReach = new List<Pawn>();
        private static List<Pawn> tmpCanOpen = new List<Pawn>();
        private static List<Pawn> tmpAllowedPawns = new List<Pawn>();
    }
}