using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public static class ThingDefGenerator_Timber
    {
        public static IEnumerable<ThingDef> ImpliedTreeDefs(bool hotReload = false)
        {

            foreach (ThingDef tree in TimberUtility.AllTreesForGenerator)
            {
                ThingDef woodDef = tree.plant.harvestedThingDef;
                string defName = TimberUtility.GetNameString(woodDef);
                ThingDef timberDef = hotReload ? DefDatabase<ThingDef>.GetNamed(defName, false) ?? new ThingDef() : new ThingDef();
                if (woodDef != null && !Utility.LogList.blackListWood.Contains(woodDef))
                {
                    if (!Utility.LogList.plankDict.ContainsValue(woodDef))
                    {
                        if (TimberUtility.WoodDefsSeen.ContainsKey(woodDef))
                        {
                            timberDef = TimberUtility.WoodDefsSeen[woodDef];
                            HideUtility.DetermineButcherProducts(tree, woodDef, timberDef);
                            tree.plant.harvestedThingDef = timberDef;
                            continue;
                        }
                        timberDef = TimberUtility.MakeHideFor(woodDef, tree);
                        TimberUtility.TryAddEntry(tree, woodDef, timberDef);
                        TimberUtility.DetermineButcherProducts(tree, woodDef, timberDef);
                        tree.plant.harvestedThingDef = timberDef;
                        TimberUtility.AllPlanks.AddDistinct(woodDef);
                        yield return timberDef;
                    }
                }
            }
            foreach (ThingDef animal in TimberUtility.AllButchered)
            {
                List<ThingDefCountClass> butcherProductList = animal.butcherProducts;

                for (int i = 0; i < butcherProductList.Count; i++)
                {
                    ThingDefCountClass butcherProduct = butcherProductList[i];
                    ThingDef product = butcherProduct.thingDef;
                    if (product != null)
                    {
                        if (TimberUtility.WoodDefsSeen.ContainsKey(product))
                        {
                            double productCount = butcherProduct.count / 2;
                            int productNum = (int)Math.Round(productCount);
                            animal.butcherProducts = new List<ThingDefCountClass>
                        {
                            new ThingDefCountClass
                            {
                                thingDef = product,
                                count = productNum
                            }
                        };

                        }
                    }
                }
            }
            foreach (ThingDef leathered in TimberUtility.AllLeatheredAnimals)
            {
                if (leathered.race.leatherDef != null)
                {
                    ThingDef woodDef = leathered.race.leatherDef;
                    if (TimberUtility.WoodDefsSeen.ContainsKey(woodDef))
                    {
                        leathered.race.leatherDef = TimberUtility.WoodDefsSeen[woodDef];
                    }
                }
            }
            foreach (ThingDef animal in TimberUtility.AllProductSpawner)
            {
                CompProperties_Spawner comp = animal.GetCompProperties<CompProperties_Spawner>();
                ThingDef woodDef = comp.thingToSpawn;
                if (woodDef != null)
                {
                    if (TimberUtility.WoodDefsSeen.ContainsKey(woodDef))
                    {

                        ThingDef timberDef = TimberUtility.WoodDefsSeen[woodDef];
                        comp.thingToSpawn = timberDef;
                    }
                }
            }
        }
    }
}