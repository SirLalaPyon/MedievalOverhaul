using System.Collections.Generic;
using Verse;

namespace MedievalOverhaul
{
    public class FuelValueProperty : DefModExtension
    {
        public int fuelValue;

        public static FuelValueProperty Get(Def def) => def.GetModExtension<FuelValueProperty>();
    }

}

