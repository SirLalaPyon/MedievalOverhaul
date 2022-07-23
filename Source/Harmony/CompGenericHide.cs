using Verse;

namespace DankPyon
{
    public class CompGenericHide : ThingComp
    {
        public ThingDef pawnSource;
        public int leatherAmount;
        public int marketValue;

        public override bool AllowStackWith(Thing other)
        {
            var comp = other.TryGetComp<CompGenericHide>();
            if (comp?.pawnSource != this.pawnSource)
            {
                return false;
            }
            return base.AllowStackWith(other);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref pawnSource, "pawnSource");
            Scribe_Values.Look(ref leatherAmount, "leatherAmount");
            Scribe_Values.Look(ref marketValue, "MarketValue");
        }
        public override string TransformLabel(string label) => pawnSource == null ? label : pawnSource.label + " " + label;
    }

}
