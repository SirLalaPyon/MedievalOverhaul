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

            foreach (ThingDef animal in MedievalOverhaul_Settings.AllLeatherAnimals)
            {
                ThingDef hideDef = new ThingDef();
                ThingDef leatherDef = animal.race.leatherDef;
                if (GeneratorUtility.LeatherDefsSeen.ContainsKey(leatherDef) && !GeneratorUtility.WhiteList.whiteListRaces.Contains(animal.defName))
                {
                    hideDef = GeneratorUtility.LeatherDefsSeen[leatherDef];
                    GeneratorUtility.DetermineButcherProducts(animal, leatherDef, hideDef, 1);
                    animal.race.leatherDef = hideDef;
                    continue;
                }
                hideDef = GeneratorUtility.MakeHideFor(leatherDef, animal);
                GeneratorUtility.TryAddEntry(animal, leatherDef, hideDef);
                GeneratorUtility.DetermineButcherProducts(animal, leatherDef, hideDef, 1);
                animal.race.leatherDef = hideDef;
                yield return hideDef;
            }
        }
    }
}