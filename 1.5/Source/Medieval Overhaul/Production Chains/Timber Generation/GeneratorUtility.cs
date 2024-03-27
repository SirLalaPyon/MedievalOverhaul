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
        public static void TryAddEntry(ThingDef tree, ThingDef wood, ThingDef timber)
        {
            if (!WoodDefsSeen.ContainsKey(tree))
            {
                timber.descriptionHyperlinks = timber.descriptionHyperlinks ?? new List<DefHyperlink>();
                timber.descriptionHyperlinks.Add(new DefHyperlink { def = wood });
                timber.modContentPack = myContentPack;
                ThingFilter filter = new ThingFilter();
                List<ThingDef> list = new List<ThingDef>
                {
                    { timber }
                };
                AccessTools.Field(typeof(ThingFilter), "thingDefs").SetValue(filter, list);
                if (!WoodDefsSeen.ContainsKey(wood))
                {
                    WoodDefsSeen.Add(wood, timber);
                }
            }
        }

        public static void DetermineButcherProducts(ThingDef tree, ThingDef wood, ThingDef timber, int number)
        {
            if (!GeneratorUtility.AnimalDefsSeen.ContainsKey(tree))
            {
                tree.butcherProducts = tree.butcherProducts ?? new List<ThingDefCountClass>();
                tree.butcherProducts.Add(new ThingDefCountClass { thingDef = timber, count = number });
            }
        }

        public static ThingDef MakeHideFor(ThingDef wood, ThingDef tree)
        {
            ThingDef timber = BasicHideDef(wood);
            SetNameAndDesc(wood, timber, tree);
            //GraphicCheck(hideDef, raceDef);
            if (wood.stuffProps != null)
            {
                timber.graphicData.color = wood.stuffProps.color;
            }
            else
            {
                timber.graphicData.color = wood.graphicData.color;
            }
            timber.butcherProducts = new List<ThingDefCountClass>
            {
                new ThingDefCountClass
                {
                    thingDef = wood,
                    count = 2
                }
            };
            wood.graphicData.texPath = "Resources/WoodPlank";
            wood.graphicData.color = timber.graphicData.color;
            if (wood.thingCategories.NullOrEmpty())
            {
                wood.thingCategories = new List<ThingCategoryDef> { };
            }
            if (!wood.thingCategories.Contains(MedievalOverhaulDefOf.DankPyon_Wood))
            {
                wood.thingCategories.Add(MedievalOverhaulDefOf.DankPyon_Wood);
            }
            return timber;
        }
        private static ThingDef BasicHideDef(ThingDef wood)
        {
            ThingDef def = new ThingDef
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
                statBases = new List<StatModifier>()
            };
            def.SetStatBaseValue(StatDefOf.Beauty, -4f);
            def.SetStatBaseValue(StatDefOf.MaxHitPoints, 30f);
            def.SetStatBaseValue(StatDefOf.Flammability, wood.GetStatValueAbstract(StatDefOf.Flammability));
            def.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
            def.SetStatBaseValue(StatDefOf.Mass, (wood.GetStatValueAbstract(StatDefOf.Mass) * 3));
            def.SetStatBaseValue(StatDefOf.MarketValue, (wood.GetStatValueAbstract(StatDefOf.MarketValue)*2));
            def.SetStatBaseValue(StatDefOf.StuffPower_Armor_Sharp, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Sharp));
            def.SetStatBaseValue(StatDefOf.StuffPower_Armor_Blunt, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Blunt));
            def.SetStatBaseValue(StatDefOf.StuffPower_Armor_Heat, wood.GetStatValueAbstract(StatDefOf.StuffPower_Armor_Heat));
            def.SetStatBaseValue(StatDefOf.StuffPower_Insulation_Cold, wood.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Cold));
            def.SetStatBaseValue(StatDefOf.StuffPower_Insulation_Heat, wood.GetStatValueAbstract(StatDefOf.StuffPower_Insulation_Heat));
            def.SetStatBaseValue(StatDefOf.SharpDamageMultiplier, wood.GetStatValueAbstract(StatDefOf.SharpDamageMultiplier));
            def.SetStatBaseValue(StatDefOf.BluntDamageMultiplier, wood.GetStatValueAbstract(StatDefOf.BluntDamageMultiplier));

            def.graphicData = new GraphicData
            {
                graphicClass = typeof(Graphic_StackCount),
                texPath = "Resources/WoodLog"
            };

            def.comps.Add(new CompProperties_Forbiddable());
            def.thingCategories = new List<ThingCategoryDef>
            {
                MedievalOverhaulDefOf.DankPyon_RawWood,
            };

            return def;
        }

        private static void SetNameAndDesc(ThingDef wood, ThingDef timber, ThingDef raceDef)
        {
            timber.defName = $"DankPyon_Log_{Utilities.RemoveSubstring(wood, "DankPyon_")}".Replace(" ", "").Replace("-", "");
            timber.label = $"{wood.label}" + " " + "DankPyon_Log".Translate();
            wood.label = $"{wood.label}" + " " + "DankPyon_Timber".Translate();
        }

    }
}