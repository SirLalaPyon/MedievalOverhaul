using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace MedievalOverhaul
{
    public static class ThingDefGenerator_Timber
    {
        public static IEnumerable<ThingDef> ImpliedTreeDefs()
        {

            foreach (ThingDef tree in TimberUtility.AllTrees)
            {
                ThingDef woodDef = tree.plant.harvestedThingDef;
                ThingDef timberDef = new ThingDef();
                if (!Utility.LogList.blackListWood.Contains(woodDef))
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
            foreach (ThingDef animal in TimberUtility.AllAnimals)
            {
                List<ThingDefCountClass> butcherProductList = animal.butcherProducts;
                
                for (int i = 0; i < butcherProductList.Count; i++)
                {
                    ThingDefCountClass butcherProduct = butcherProductList[i];
                    ThingDef product = butcherProduct.thingDef;
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
                if (animal.race.leatherDef != null)
                {
                    ThingDef woodDef = animal.race.leatherDef;
                    if (TimberUtility.WoodDefsSeen.ContainsKey(woodDef))
                    {
                        animal.race.leatherDef = TimberUtility.WoodDefsSeen[woodDef];
                    }
                }
            }
            foreach (ThingDef animal in TimberUtility.AllProductSpawner)
            {
                CompProperties_Spawner comp = animal.GetCompProperties<CompProperties_Spawner>();
                ThingDef woodDef = comp.thingToSpawn;
                if (TimberUtility.WoodDefsSeen.ContainsKey(woodDef))
                {
                    
                    ThingDef timberDef = TimberUtility.WoodDefsSeen[woodDef];
                    comp.thingToSpawn = timberDef;
                }
            }
        }
    }
}