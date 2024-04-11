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
    public static class ThingDefGenerator_Hide
    {
        public static IEnumerable<ThingDef> ImpliedHideDefs()
        {
            foreach (ThingDef animal in HideUtility.AllLeatherAnimals)
            {
                var animalName = animal.defName;
                ThingDef leatherDef = animal.race.leatherDef;
                if (!Utility.WhiteList.blackListRaces.Contains(animalName) && !Utility.WhiteList.blackListLeathers.Contains(leatherDef.defName))
                {
                    ThingDef hideDef = new ThingDef();

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
        }
    }
}