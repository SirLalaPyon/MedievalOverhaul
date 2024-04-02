using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul.Wood
{
    public static class GeneratorUtilities
    {
        public static List<ThingDef> AllTrees = new List<ThingDef>();
        public static List<ThingDef> AllAnimals = new List<ThingDef>();
        public static List<ThingDef> AllProductSpawner = new List<ThingDef>();
        public static Dictionary<ThingDef, ThingDef> WoodDefsSeen = new Dictionary<ThingDef, ThingDef>();
        private static ModContentPack myContentPack = LoadedModManager.GetMod<MedievalOverhaulSettings>().Content;
        public static void MakeListOfTrees()
        {
            if (MedievalOverhaulSettings.settings.woodChain)
            {
                foreach (ThingDef tree in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Plant).ToList())
                {
                    if (tree.plant.harvestTag == "Wood")
                    {
                        AllTrees.Add(tree);
                    }
                }
                foreach (ThingDef animal in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Pawn).ToList())
                {
                    if (animal.butcherProducts != null)
                    {
                        AllAnimals.Add(animal);
                    }
                    if (animal.comps.Any(x => x.compClass == typeof(CompSpawner)))
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
                log.modContentPack = myContentPack;
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

        public static void DetermineButcherProducts(ThingDef tree, ThingDef wood, ThingDef log, int number)
        {
            if (!GeneratorUtility.AnimalDefsSeen.ContainsKey(tree))
            {
                tree.butcherProducts = tree.butcherProducts ?? new List<ThingDefCountClass>();
                tree.butcherProducts.Add(new ThingDefCountClass { thingDef = log, count = number });
            }
        }

        public static ThingDef MakeHideFor(ThingDef wood, ThingDef tree)
        {
            ThingDef log = BasicLogDef(wood);
            SetNameAndDesc(wood, log, tree);
            //GraphicCheck(hideDef, raceDef);
            if (wood.stuffProps != null)
            {
                log.graphicData.color = wood.stuffProps.color;
            }
            else
            {
                log.graphicData.color = wood.graphicData.color;
            }
            log.butcherProducts = new List<ThingDefCountClass>
            {
                new ThingDefCountClass
                {
                    thingDef = wood,
                    count = 2
                }
            };
            wood.graphicData.texPath = "Resources/WoodPlank";
            wood.graphicData.color = log.graphicData.color;
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
                stackLimit = 100,
                alwaysHaulable = true,
                drawGUIOverlay = true,
                rotatable = false,
                pathCost = 14,
                allowedArchonexusCount = -1,
                tickerType = TickerType.Rare,
                healthAffectsPrice = false,
                soundInteract = SoundDefOf.Standard_Drop,
                statBases = new List<StatModifier>(),
            };
            log.SetStatBaseValue(StatDefOf.Beauty, -4f);
            log.SetStatBaseValue(StatDefOf.MaxHitPoints, 30f);
            log.SetStatBaseValue(StatDefOf.Flammability, wood.GetStatValueAbstract(StatDefOf.Flammability));
            log.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
            log.SetStatBaseValue(StatDefOf.Mass, (wood.GetStatValueAbstract(StatDefOf.Mass) * 3));
            log.SetStatBaseValue(StatDefOf.MarketValue, (wood.GetStatValueAbstract(StatDefOf.MarketValue) * 2));
            log.SetStatBaseValue(StatDefOf.StuffPower_Armor_Sharp, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Sharp));
            log.SetStatBaseValue(StatDefOf.StuffPower_Armor_Blunt, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Blunt));
            log.SetStatBaseValue(StatDefOf.StuffPower_Armor_Heat, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Heat));
            log.SetStatBaseValue(StatDefOf.StuffPower_Insulation_Cold, wood.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold));
            log.SetStatBaseValue(StatDefOf.StuffPower_Insulation_Heat, wood.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Heat));
            log.SetStatBaseValue(StatDefOf.SharpDamageMultiplier, wood.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier));
            log.SetStatBaseValue(StatDefOf.BluntDamageMultiplier, wood.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier));

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
                stuffAdjective = wood.stuffProps.stuffAdjective.ToString() + " " + "DankPyon_Log".Translate(),
                constructEffect = wood.stuffProps.constructEffect,
                soundMeleeHitBlunt = wood.stuffProps.soundMeleeHitBlunt,
                soundMeleeHitSharp = wood.stuffProps.soundMeleeHitSharp,
                color = wood.stuffProps.color,
                statFactors = wood.stuffProps.statFactors,
            };
            log.comps.Add(new CompProperties_FuelRate()
            {
                rate = 2f,
            });

            return log;
        }

        private static void SetNameAndDesc(ThingDef wood, ThingDef timber, ThingDef raceDef)
        {
            timber.defName = $"DankPyon_Log_{Utilities.RemoveSubstring(wood, "DankPyon_")}".Replace(" ", "").Replace("-", "");
            timber.label = $"{wood.label}" + " " + "DankPyon_Log".Translate();
            wood.label = $"{wood.label}" + " " + "DankPyon_Timber".Translate();
        }

    }
}