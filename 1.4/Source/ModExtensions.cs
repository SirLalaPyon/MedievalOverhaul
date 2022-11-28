using System.Collections.Generic;
using Verse;

namespace DankPyon
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
    public class CannotBePlacedTogetherWithThisModExtension : DefModExtension
    {

    }
}
