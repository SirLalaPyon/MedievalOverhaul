using MedievalOverhaul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VanillaGenesExpanded;

namespace MedievalOverhaul
{
    public static class ThingDefGenerator_Hide
    {
        public static IEnumerable<ThingDef> ImpliedHideDefs(bool hotReload = false)
        {
            foreach (ThingDef animal in HideUtility.AllLeatherAnimals)
            {
                var animalName = animal.defName;
                ThingDef leatherDef = animal.race.leatherDef;
                if (!Utility.WhiteList.blackListRaces.Contains(animalName) && !Utility.WhiteList.blackListLeathers.Contains(leatherDef.defName))
                {
                    string defName = HideUtility.GetNameString(leatherDef, animal);
                    ThingDef hideDef = hotReload ? DefDatabase<ThingDef>.GetNamed(defName, false) ?? new ThingDef() : new ThingDef();

                    if (HideUtility.LeatherDefsSeen.ContainsKey(leatherDef) && !Utility.WhiteList.whiteListRaces.Contains(animalName))
                    {
                        hideDef = HideUtility.LeatherDefsSeen[leatherDef];
                        HideUtility.DetermineButcherProducts(animal, leatherDef, hideDef);
                        animal.race.leatherDef = hideDef;
                        continue;
                    }
                    hideDef = HideUtility.MakeHideFor(leatherDef, animal);
                    HideUtility.TryAddEntry(animal, leatherDef, hideDef);
                    HideUtility.DetermineButcherProducts(animal, leatherDef, hideDef);
                    animal.race.leatherDef = hideDef;
                    yield return hideDef;
                }
            }
            if (MedievalOverhaulSettings.settings.leatherChain)
            {
                foreach (GeneDef gene in HideUtility.AllGeneDefs)
                {
                    ThingDef geneLeather = gene.GetModExtension<GeneExtension>().customLeatherThingDef;
                    if (HideUtility.LeatherDefsSeen.ContainsKey(geneLeather))
                    {
                        gene.GetModExtension<GeneExtension>().customLeatherThingDef = HideUtility.LeatherDefsSeen[geneLeather];
                    }
                }
            }
        }
    }
}