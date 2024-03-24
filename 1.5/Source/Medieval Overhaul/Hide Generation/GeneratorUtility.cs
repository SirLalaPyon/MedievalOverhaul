using HarmonyLib;
using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public static class GeneratorUtility
    {
        public static Dictionary<ThingDef, ThingDef> LeatherDefsSeen = new Dictionary<ThingDef, ThingDef>();
        public static Dictionary<ThingDef, ThingDef> AnimalDefsSeen = new Dictionary<ThingDef, ThingDef>();
        public static SeperateHideList WhiteList = DefDatabase<SeperateHideList>.GetNamed("WhiteList");
        private static ModContentPack myContentPack = LoadedModManager.GetMod<MedievalOverhaulSettings>().Content;

        public static void MakeListOfShearables()
        {
            if (MedievalOverhaulSettings.settings.leatherChain)
            {
                foreach (ThingDef animal in DefDatabase<ThingDef>.AllDefs.Where(x => x.category == ThingCategory.Pawn).ToList())
                {
                    if (animal.race.leatherDef != null && animal.race.IsFlesh && !animal.race.Insect)
                    {
                        MedievalOverhaul_Settings.AllLeatherAnimals.Add(animal);
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
                hide.modContentPack = myContentPack;
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

        private static ThingDef BasicHideDef()
        {
            ThingDef def = new ThingDef
            {
                description = "DankPyon_Hide_Description".Translate(),
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
            def.SetStatBaseValue(StatDefOf.Flammability, 1f);
            def.SetStatBaseValue(StatDefOf.DeteriorationRate, 2f);
            def.SetStatBaseValue(StatDefOf.Mass, 0.03f);
            def.SetStatBaseValue(StatDefOf.MarketValue, 1f);

            def.graphicData = new GraphicData
            {
                graphicClass = typeof(Graphic_StackCount),
                texPath = "Resources/HeavyFurMedium"
            };

            def.comps.Add(new CompProperties_Forbiddable());
            def.thingCategories = new List<ThingCategoryDef>
            {
                MedievalOverhaulDefOf.DankPyon_Hides,
            };

            return def;
        }

        public static void DetermineButcherProducts(ThingDef animal, ThingDef leatherDef, ThingDef hideDef, int number)
        {
            if (!GeneratorUtility.AnimalDefsSeen.ContainsKey(animal))
            {
                animal.butcherProducts = animal.butcherProducts ?? new List<ThingDefCountClass>();

                //int half = (int)Math.Round(number / 2f);
                //int mod = half % 5;
                //mod = (mod == 0) ? 5 : mod;
                //int count = half + (5 - mod);
               // Log.Message("Animal: " + animal + " Old Leather: " + leatherDef + " New Hide: " + hideDef);
                animal.butcherProducts.Add(new ThingDefCountClass { thingDef = hideDef, count = number });
            }
        }

        public static ThingDef MakeHideFor(ThingDef leatherDef, ThingDef raceDef)
        {
            ThingDef hideDef = BasicHideDef();
            SetNameAndDesc(leatherDef, hideDef, raceDef);
            //GraphicCheck(hideDef, raceDef);
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
            //else if (leatherDef.defName == "Leather_Bird")
            //{
            //    hideDef.defName = $"DankPyon_Hide_BirdMedium".Replace(" ", "").Replace("-", "");
            //    hideDef.label = $"Medium birdskin";
            //}
            //else if (leatherDef.defName == "Leather_Bear")
            //{

            //}
            else if (WhiteList.whiteListRaces.Contains(raceDef.defName) || WhiteList.whiteListLeathers.Contains(leatherDef.defName))
            {
                hideDef.defName = $"DankPyon_Hide_{Utilities.RemoveSubstring(raceDef, "DankPyon_")}".Replace(" ", "").Replace("-", "");
                hideDef.label = $"{raceDef.label}" + " " + "DankPyon_Hide".Translate();
            }
            else
            {
                hideDef.defName = $"DankPyon_Hide_{Utilities.RemoveSubstring(leatherDef, "DankPyon_")}".Replace(" ", "").Replace("-", "");
                hideDef.label = $"{leatherDef.label}" + " " + "DankPyon_Hide".Translate();
            }
        }

        //private static void GraphicCheck (ThingDef hideDef, ThingDef raceDef)
        //{
        //    if (raceDef.race.body.defName == "Bird" && raceDef.race.baseBodySize >= 1)
        //    {
        //        hideDef.graphicData.texPath = "Resources/ScaleMedium";
        //    }
        //}
        //internal static SeperateHideList WhiteList;

    }
}
