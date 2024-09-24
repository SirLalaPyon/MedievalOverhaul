using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public static class TimberUtility
    {
        public static List<ThingDef> AllTreesForGenerator = new List<ThingDef>();
        public static List<ThingDef> AllLeatheredAnimals = new List<ThingDef>();
        public static List<ThingDef> AllButchered = new List<ThingDef>();
        public static List<ThingDef> AllProductSpawner = new List<ThingDef>();
        public static Dictionary<ThingDef, ThingDef> WoodDefsSeen = new Dictionary<ThingDef, ThingDef>();
        public static List<ThingDef> AllPlanks = new List<ThingDef>();
        public static void MakeListOfTrees()
        {
            if (MedievalOverhaulSettings.settings.woodChain)
            {
                foreach (ThingDef tree in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Plant).ToList())
                {
                    if (tree.plant?.harvestTag == "Wood" && (tree.plant?.harvestedThingDef?.stuffProps?.categories?.Contains(StuffCategoryDefOf.Woody) ?? false))
                    {
                        AllTreesForGenerator.Add(tree);
                    }
                }
                foreach (ThingDef animal in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Pawn).ToList())
                {
                    if (animal.butcherProducts != null)
                    {
                        AllButchered.Add(animal);
                    }
                    if (animal.race?.leatherDef != null)
                    {
                        AllLeatheredAnimals.Add(animal);
                    }

                    if (animal.comps?.Any(x => x.compClass == typeof(CompSpawner)) ?? false)
                    {
                        AllProductSpawner.Add(animal);
                    }
                }
            }
        }
        public static void TryAddEntry(ThingDef tree, ThingDef wood, ThingDef log)
        {
            if (!WoodDefsSeen.ContainsKey(tree))
            {
                log.descriptionHyperlinks = log.descriptionHyperlinks ?? new List<DefHyperlink>();
                log.descriptionHyperlinks.Add(new DefHyperlink { def = wood });
                log.modContentPack = Utility.myContentPack;
                ThingFilter filter = new ThingFilter();
                List<ThingDef> list = new List<ThingDef>
                {
                    { log }
                };
                AccessTools.Field(typeof(ThingFilter), "thingDefs").SetValue(filter, list);
                if (!WoodDefsSeen.ContainsKey(wood))
                {
                    WoodDefsSeen.Add(wood, log);
                }
            }
        }

        public static void DetermineButcherProducts(ThingDef tree, ThingDef wood, ThingDef log)
        {
            if (!HideUtility.AnimalDefsSeen.ContainsKey(tree))
            {
                tree.butcherProducts = tree.butcherProducts ?? new List<ThingDefCountClass>();
                tree.butcherProducts.Add(new ThingDefCountClass { thingDef = log, count = 2 });
            }
        }

        public static ThingDef MakeHideFor(ThingDef wood, ThingDef tree)
        {
            ThingDef log = BasicLogDef(wood);
            SetNameAndDesc(wood, log);
            try
            {
                if (wood.stuffProps != null)
                {
                    log.graphicData.color = wood.stuffProps.color;
                }
                else
                {
                    log.graphicData.color = wood.graphicData.color;
                }
            }
            catch
            {
                Log.Message("Error with generating graphicData for: " + log + "with base thingDef of " + wood);
            }
            try
            {
                log.butcherProducts = new List<ThingDefCountClass>
            {
                new ThingDefCountClass
                {
                    thingDef = wood,
                    count = 2
                }
            };
            }
            catch
            {
                Log.Message("Error generating butcher products for: " + log + "with base thingDef of " + wood);
            }
            try
            {
                wood.graphicData.texPath = "Resources/WoodPlank";
                wood.graphicData.color = log.graphicData.color;
                wood.stuffProps.stuffAdjective = wood.stuffProps.stuffAdjective.ToString() + " " + "DankPyon_Plank".Translate();
            }
            catch
            {
                Log.Message("Error generating more graphicData for: " + log + "with base thingDef of " + wood);
            }
            try
            {
                if (wood.thingCategories.NullOrEmpty())
                {
                    wood.thingCategories = new List<ThingCategoryDef> { };
                }
                if (wood.thingCategories.Contains(ThingCategoryDefOf.ResourcesRaw))
                {
                    List<ThingCategoryDef> thingCategory = wood.thingCategories;
                    List<ThingCategoryDef> newThingCategory = new List<ThingCategoryDef> { };
                    for (int i = 0; i < thingCategory.Count; i++)
                    {
                        ThingCategoryDef thing = thingCategory[i];

                        if (thing != ThingCategoryDefOf.ResourcesRaw)
                        {
                            newThingCategory.Add(thing);
                        }
                    }
                    wood.thingCategories = newThingCategory;
                }
                if (!wood.thingCategories.Contains(MedievalOverhaulDefOf.DankPyon_Wood))
                {
                    wood.thingCategories.Add(MedievalOverhaulDefOf.DankPyon_Wood);
                }
            }
            catch
            {
                Log.Message("Error generating thingCategories for: " + log + "with base thingDef of " + wood);
            }
            return log;
        }
        private static ThingDef BasicLogDef(ThingDef wood)
        {
            ThingDef log = new ThingDef
            {
                description = "DankPyon_Plank_Description".Translate(),
                thingClass = typeof(ThingWithComps),
                category = ThingCategory.Item,
                drawerType = DrawerType.MapMeshOnly,
                resourceReadoutPriority = ResourceCountPriority.Middle,
                useHitPoints = true,
                selectable = true,
                alwaysHaulable = true,
                drawGUIOverlay = true,
                rotatable = false,
                pathCost = 14,
                allowedArchonexusCount = -1,
                tickerType = TickerType.Rare,
                healthAffectsPrice = false,
                soundInteract = SoundDefOf.Standard_Drop,
                statBases = new List<StatModifier>(),
                terrainAffordanceNeeded = TerrainAffordanceDefOf.Light,
                isTechHediff = true,
                burnableByRecipe = true,
                minRewardCount = 10,
            };
            log.stackLimit = wood.stackLimit;
            log.SetStatBaseValue(StatDefOf.Beauty, -4f);
            log.SetStatBaseValue(StatDefOf.MaxHitPoints, 30f);
            log.SetStatBaseValue(StatDefOf.Flammability, wood.GetStatValueAbstract(StatDefOf.Flammability));
            log.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
            log.SetStatBaseValue(StatDefOf.Mass, (wood.GetStatValueAbstract(StatDefOf.Mass) * 3));
            log.SetStatBaseValue(StatDefOf.MarketValue, (wood.GetStatValueAbstract(StatDefOf.MarketValue) * 2));
            log.SetStatBaseValue(StatDefOf.StuffPower_Armor_Sharp, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Sharp));
            log.SetStatBaseValue(StatDefOf.StuffPower_Armor_Blunt, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Blunt));
            if (Utility.CEIsEnabled)
            {
                log.SetStatBaseValue(MedievalOverhaulDefOf.Bulk, wood.GetStatValueAbstract(MedievalOverhaulDefOf.Bulk)*2);
            }
            log.SetStatBaseValue(StatDefOf.StuffPower_Armor_Heat, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Heat));
            log.SetStatBaseValue(StatDefOf.StuffPower_Insulation_Cold, wood.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold));
            log.SetStatBaseValue(StatDefOf.StuffPower_Insulation_Heat, wood.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Heat));
            log.SetStatBaseValue(StatDefOf.SharpDamageMultiplier, wood.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier));
            log.SetStatBaseValue(StatDefOf.BluntDamageMultiplier, wood.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier));
            log.tools = wood.tools;
            log.comps = wood.comps;
            log.techLevel = wood.techLevel;
            log.equipmentType = wood.equipmentType;
            if (wood.weaponClasses != null)
            {
                log.weaponClasses = wood.weaponClasses;
            }
            if (wood.techHediffsTags != null)
            {
                log.techHediffsTags = wood.techHediffsTags;
            }
            log.graphicData = new GraphicData
            {
                graphicClass = typeof(Graphic_StackCount),
                texPath = "Resources/WoodLog"
            };

            log.comps.Add(new CompProperties_Forbiddable());
            log.thingCategories = new List<ThingCategoryDef>
            {
                MedievalOverhaulDefOf.DankPyon_RawWood,

            };


            log.stuffProps = new StuffProperties
            {
                categories = new List<StuffCategoryDef>
                {
                   StuffabilityDefOf.DankPyon_RawWood,
                },
                stuffAdjective = wood.stuffProps.stuffAdjective.ToString(),
                constructEffect = wood.stuffProps.constructEffect,
                soundImpactBullet = wood?.stuffProps?.soundImpactBullet ?? MedievalOverhaulDefOf.BulletImpact_Wood,
                soundMeleeHitBlunt = wood?.stuffProps?.soundMeleeHitBlunt ?? MedievalOverhaulDefOf.MeleeHit_Wood,
                soundMeleeHitSharp = wood?.stuffProps?.soundMeleeHitSharp ?? MedievalOverhaulDefOf.MeleeHit_Wood,
                soundImpactMelee = wood?.stuffProps?.soundImpactMelee ?? MedievalOverhaulDefOf.Pawn_Melee_Punch_HitBuilding_Wood,
                color = wood.stuffProps.color,
                statFactors = wood.stuffProps.statFactors,
            };
            log.modExtensions = new List<DefModExtension>
            {
                new FuelValueProperty()
                {
                    fuelValue = 2,
                },
            };


            return log;
        }

        private static void SetNameAndDesc(ThingDef wood, ThingDef timber)
        {
            timber.defName = $"DankPyon_Log_{Utility.RemoveSubstring(wood, "DankPyon_")}".Replace(" ", "").Replace("-", "");
            //timber.label = $"{wood.label}" + " " + "DankPyon_Log".Translate();
            //wood.label = $"{wood.label}" + " " + "DankPyon_Timber".Translate();
            timber.label = "DankPyon_Log".Translate(wood.label);
            wood.label = "DankPyon_Timber".Translate(wood.label);

        }

        public static string GetNameString(ThingDef wood)
        {
            string stringDefName = $"DankPyon_Log_{Utility.RemoveSubstring(wood, "DankPyon_")}".Replace(" ", "").Replace("-", "");
            return stringDefName;
        }

    }
}