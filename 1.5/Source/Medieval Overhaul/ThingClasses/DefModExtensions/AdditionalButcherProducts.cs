using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MedievalOverhaul
{
    public class ButcherOption
    {
        public ThingDef thingDef;
        public IntRange amount;
        public float chance;
    }
    public class AdditionalButcherProducts : DefModExtension
    {
        public List<ButcherOption> butcherOptions;
    }
}
