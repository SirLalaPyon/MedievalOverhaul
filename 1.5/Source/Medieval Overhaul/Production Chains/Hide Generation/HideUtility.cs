using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MedievalOverhaul
{
    public static class HideUtility
    {
        public static List<ThingDef> AllLeatherAnimals = new List<ThingDef>();
        public static Dictionary<ThingDef, ThingDef> LeatherDefsSeen = new Dictionary<ThingDef, ThingDef>();
        public static Dictionary<ThingDef, ThingDef> AnimalDefsSeen = new Dictionary<ThingDef, ThingDef>();

        public static void MakeListOfAnimals()
        {
            if (MedievalOverhaulSettings.settings.leatherChain)
            {
                foreach (ThingDef animal in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Pawn).ToList())
                {
                    if (animal.race.leatherDef != null && animal.race.IsFlesh && !animal.race.Insect)
                    {
                        AllLeatherAnimals.Add(animal);
                    }
                }
            }
        }

        public static void TryAddEntry(ThingDef animal, ThingDef leather, ThingDef hide)
        {
            if (!AnimalDefsSeen.ContainsKey(animal))
            {
                hide.descriptionHyperlinks = hide.descriptionHyperlinks ?? new List<DefHyperlink>();
                hide.descriptionHyperlinks.Add(new DefHyperlink { def = leather });
                hide.modContentPack = Utility.myContentPack;
                ThingFilter filter = new ThingFilter();
                List<ThingDef> list = new List<ThingDef>
                {
                    { hide }
                };
                AccessTools.Field(typeof(ThingFilter), "thingDefs").SetValue(filter, list);
                if (!LeatherDefsSeen.ContainsKey(leather))
                {
                    LeatherDefsSeen.Add(leather, hide);
                }
                if (!AnimalDefsSeen.ContainsKey(hide))
                {
                    AnimalDefsSeen.Add(animal, hide);
                }
            }
        }

        private static ThingDef BasicHideDef(ThingDef raceDef, ThingDef leatherDef)
        {
            string defName = "DankPyon_" + raceDef.defName;
            ThingDef def = (DefDatabase<ThingDef>.GetNamed(defName, false) ?? new ThingDef());
            def.description = "DankPyon_Hide_Description".Translate();
            def.thingClass = typeof(ThingWithComps);
            def.category = ThingCategory.Item;
            def.drawerType = DrawerType.MapMeshOnly;
            def.resourceReadoutPriority = ResourceCountPriority.Middle;
            def.useHitPoints = true;
            def.selectable = true;
            def.stackLimit = 100;
            def.alwaysHaulable = true;
            def.drawGUIOverlay = true;
            def.rotatable = false;
            def.pathCost = 14;
            def.allowedArchonexusCount = -1;
            def.tickerType = TickerType.Rare;
            def.healthAffectsPrice = false;
            def.soundInteract = SoundDefOf.Standard_Drop;
            def.statBases = new List<StatModifier>();
            def.SetStatBaseValue(StatDefOf.Beauty, -4f);
            def.SetStatBaseValue(StatDefOf.MaxHitPoints, 30f);
            def.SetStatBaseValue(StatDefOf.Flammability, 1f);
            def.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
            def.SetStatBaseValue(StatDefOf.Mass, 0.03f);
            def.SetStatBaseValue(StatDefOf.MarketValue, 1f);
            def.comps.Add(new CompProperties_Forbiddable());
            def.comps.Add(new CompProperties_GenericHide()
            {
                pawnSource = raceDef,
                leatherType = leatherDef
            }) ;
            def.comps.Add(new CompProperties_Rottable()
            {
                daysToRotStart = 5,
                rotDestroys = true
            });
            if (Utility.CEIsEnabled)
            {
                def.SetStatBaseValue(MedievalOverhaulDefOf.Bulk, leatherDef.GetStatValueAbstract(MedievalOverhaulDefOf.Bulk));
            }
            string texPathString = GetHideGraphic(raceDef);
            def.graphicData = new GraphicData
            {
                graphicClass = typeof(Graphic_StackCount),
                texPath = texPathString
            };
            def.thingCategories = new List<ThingCategoryDef>
            {
                MedievalOverhaulDefOf.DankPyon_Hides,
            };

            return def;
        }

        public static void DetermineButcherProducts(ThingDef animal, ThingDef leatherDef, ThingDef hideDef)
        {
            if (!HideUtility.AnimalDefsSeen.ContainsKey(animal))
            {
                animal.butcherProducts = animal.butcherProducts ?? new List<ThingDefCountClass>();
                animal.butcherProducts.Add(new ThingDefCountClass { thingDef = hideDef, count = 1 });
            }
        }

        public static ThingDef MakeHideFor(ThingDef leatherDef, ThingDef raceDef)
        {
            ThingDef hideDef = BasicHideDef(raceDef, leatherDef);
            SetNameAndDesc(leatherDef, hideDef, raceDef);
            if (leatherDef.stuffProps != null)
            {
                hideDef.graphicData.color = leatherDef.stuffProps.color;
            }
            else
            {
                hideDef.graphicData.color = leatherDef.graphicData.color;
            }
            hideDef.butcherProducts = new List<ThingDefCountClass>
            {
                new ThingDefCountClass
                {
                    thingDef = leatherDef,
                    count = 1
                }
            };
            return hideDef;
        }

        private static void SetNameAndDesc(ThingDef leatherDef, ThingDef hideDef, ThingDef raceDef)
        {

            if (leatherDef.defName == "Leather_Plain")
            {
                hideDef.defName = $"DankPyon_Hide_Plain".Replace(" ", "").Replace("-", "");
                hideDef.label = $"Plain hide";
            }
            else if (Utility.WhiteList.whiteListRaces.Contains(raceDef.defName) || Utility.WhiteList.whiteListLeathers.Contains(leatherDef.defName))
            {
                hideDef.defName = $"DankPyon_Hide_{Utility.RemoveSubstring(raceDef, "DankPyon_")}".Replace(" ", "").Replace("-", "");
                hideDef.label = $"{raceDef.label}" + " " + "DankPyon_Hide".Translate();
            }
            else
            {
                hideDef.defName = $"DankPyon_Hide_{Utility.RemoveSubstring(leatherDef, "DankPyon_")}".Replace(" ", "").Replace("-", "");
                hideDef.label = $"{leatherDef.label}" + " " + "DankPyon_Hide".Translate();
            }
        }

        public static string GetNameString(ThingDef leatherDef, ThingDef raceDef)
        {
            string defNameString;
            if (leatherDef.defName == "Leather_Plain")
            {
                defNameString = $"DankPyon_Hide_Plain".Replace(" ", "").Replace("-", "");
            }
            else if (Utility.WhiteList.whiteListRaces.Contains(raceDef.defName) || Utility.WhiteList.whiteListLeathers.Contains(leatherDef.defName))
            {
                defNameString = $"DankPyon_Hide_{Utility.RemoveSubstring(raceDef, "DankPyon_")}".Replace(" ", "").Replace("-", "");
            }
            else
            {
                defNameString = $"DankPyon_Hide_{Utility.RemoveSubstring(leatherDef, "DankPyon_")}".Replace(" ", "").Replace("-", "");
            }
            return defNameString;
        }

        private static string GetHideGraphic(ThingDef raceDef)
        {
            string texPathString = "Resources/HeavyFurMedium";
            float bodySize = raceDef.race.baseBodySize;
            string bodyDef = raceDef.race.body.ToString();
            string defName = raceDef.ToString();
            if (Utility.HideGraphicList.scaleHidesBodyDef.Contains(bodyDef) || Utility.HideGraphicList.scaleHidesRaceDef.Contains(defName))
            {
                if (bodySize < 0.5)
                {
                    texPathString = "Resources/ScaleTiny";
                    return texPathString;
                }
                if (bodySize < 1)
                {
                    texPathString = "Resources/ScaleSmall";
                    return texPathString;
                }
                if (bodySize < 2)
                {
                    texPathString = "Resources/ScaleMedium";
                    return texPathString;
                }
                //if (bodySize < 3)
                //{
                //    texPathString = "Resources/ScaleLarge";
                //    return texPathString;
                //}
                //texPathString = "Resources/ScaleHuge";
                //    return texPathString;
            }
            if (Utility.HideGraphicList.furHidesBodyDef.Contains(bodyDef) || Utility.HideGraphicList.scaleHidesRaceDef.Contains(defName))
            {
                if (bodySize < 0.5)
                {
                    texPathString = "Resources/HeavyFurTiny";
                    return texPathString;
                }
                if (bodySize < 1)
                {
                    texPathString = "Resources/HeavyFurSmall";
                    return texPathString;
                }
                if (bodySize < 2)
                {
                    texPathString = "Resources/HeavyFurMedium";
                    return texPathString;
                }
                //if (bodySize < 3)
                //{
                //    texPathString = "Resources/HeavyFurHuge";
                //    return texPathString;
                //}
                //    texPathString = "Resources/HeavyFurHuge";
                //    return texPathString;
            }
            if (bodySize < 0.5)
            {
                texPathString = "Resources/HideTiny";
                return texPathString;
            }
            if (bodySize < 1)
            {
                texPathString = "Resources/HideTiny";
                return texPathString;
            }
            return texPathString;
        }
        public static bool IsHide(ThingDef thing)
        {
                return thing.category == ThingCategory.Item && thing.thingCategories != null && thing.thingCategories.Contains(MedievalOverhaulDefOf.DankPyon_Hides);
        }
    }
}
