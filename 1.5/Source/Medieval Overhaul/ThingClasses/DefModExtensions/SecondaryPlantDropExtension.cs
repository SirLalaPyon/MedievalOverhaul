using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class SecondaryPlantDropExtension : DefModExtension
    {
        public ThingDef secondaryDrop = null;
        public IntRange secondaryDropAmountRange;
        public float secondaryDropChance = 1f;
        public bool secondaryNotWhenLeafless = false;
        public static SecondaryPlantDropExtension Get(Def def)
        {
            return def.GetModExtension<SecondaryPlantDropExtension>();
        }
    }
}
