using MedievalOverhaul;
using MedievalOverhaul.Wood;
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

            foreach (ThingDef tree in GeneratorUtilities.AllTrees)
            {
                ThingDef woodDef = tree.plant.harvestedThingDef;
                ThingDef timberDef = new ThingDef();

                if (GeneratorUtilities.WoodDefsSeen.ContainsKey(woodDef))
                {
                    timberDef = GeneratorUtilities.WoodDefsSeen[woodDef];
                    GeneratorUtility.DetermineButcherProducts(tree, woodDef, timberDef, 1);
                    tree.plant.harvestedThingDef = timberDef;
                    continue;
                }
                timberDef = GeneratorUtilities.MakeHideFor(woodDef, tree);
                GeneratorUtilities.TryAddEntry(tree, woodDef, timberDef);
                GeneratorUtilities.DetermineButcherProducts(tree, woodDef, timberDef, 1);
                tree.plant.harvestedThingDef = timberDef;
                yield return timberDef;
            }
            foreach (ThingDef animal in GeneratorUtilities.AllAnimals)
            {
                List<ThingDefCountClass> butcherProductList = animal.butcherProducts;
                
                for (int i = 0; i < butcherProductList.Count; i++)
                {
                    ThingDefCountClass butcherProduct = butcherProductList[i];
                    ThingDef product = butcherProduct.thingDef;
                    if (GeneratorUtilities.WoodDefsSeen.ContainsKey(product))
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
                    if (GeneratorUtilities.WoodDefsSeen.ContainsKey(woodDef))
                    {
                        animal.race.leatherDef = GeneratorUtilities.WoodDefsSeen[woodDef];
                    }
                }
            }
            foreach (ThingDef animal in GeneratorUtilities.AllProductSpawner)
            {
                CompProperties_Spawner comp = animal.GetCompProperties<CompProperties_Spawner>();
                ThingDef woodDef = comp.thingToSpawn;
                if (GeneratorUtilities.WoodDefsSeen.ContainsKey(woodDef))
                {
                    
                    ThingDef timberDef = GeneratorUtilities.WoodDefsSeen[woodDef];
                    comp.thingToSpawn = timberDef;
                }
            }
        }
    }
}