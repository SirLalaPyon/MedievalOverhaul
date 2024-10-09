using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MedievalOverhaul
{
    [StaticConstructorOnStartup]
    public static class CachingUtility
    {
        public static Dictionary<ThingDef, float> FuelValueDict = [];

        static CachingUtility()
        {

            CacheFuelValue();


        }

        public static void CacheFuelValue()
        {
            foreach (ThingDef fuelThing in DefDatabase<ThingDef>.AllDefs.Where(x => x?.GetModExtension<FuelValueProperty>()?.fuelValue != null).ToList())
            {
                float fuelValue = fuelThing.GetModExtension<FuelValueProperty>().fuelValue;
                FuelValueDict.Add(fuelThing, fuelValue);
            }
        }

    }
}
