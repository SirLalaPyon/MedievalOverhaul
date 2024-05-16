using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class Item
    {
        public ThingDef thingDef;
        public IntRange countRange;
        public int weight = 1;
        public ThingDef stuffDef = (ThingDef)null;
    }
}
